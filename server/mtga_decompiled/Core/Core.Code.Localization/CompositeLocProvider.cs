using System;
using System.Collections.Generic;
using System.Linq;
using Wizards.Arena.Client.Logging;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Core.Code.Localization;

public class CompositeLocProvider : IClientLocProvider, IDisposable
{
	private readonly Logger _logger;

	private readonly List<IClientLocProvider> _nestedProviders = new List<IClientLocProvider>(3);

	private bool _disposed;

	private static readonly int MaxDepth = 1;

	private bool IsPreProd = Pantry.Get<IAccountClient>()?.IsPreProd ?? false;

	public uint ProviderCount => (uint)_nestedProviders.Count;

	public CompositeLocProvider(Logger logger = null, params IClientLocProvider[] nestedProviders)
	{
		_logger = logger ?? new UnityPassthroughLogger();
		_nestedProviders.AddRange(nestedProviders);
	}

	public void InsertProvider(IClientLocProvider newProvider)
	{
		_nestedProviders.Insert(0, newProvider);
	}

	public string GetLocalizedText(string key, params (string, string)[] locParams)
	{
		return GetLocalizedTextForLanguage(key, Languages.CurrentLanguage, locParams);
	}

	public bool TryGetLocalizedTextForLanguage(string key, string overrideLangCode, (string, string)[] locParams, out string loc)
	{
		if (_disposed)
		{
			if (IsPreProd)
			{
				throw new InvalidOperationException("CompositeLocProvider accessed after being disposed!");
			}
			loc = "ERROR: DISPOSED";
			return false;
		}
		return TryGetLocalizedTextForLanguageInternal(_nestedProviders, key, overrideLangCode, locParams, IsPreProd, 0, MaxDepth, out loc);
	}

	public static bool TryGetLocalizedTextForLanguageInternal(List<IClientLocProvider> nestedProviders, string key, string overrideLangCode, (string, string)[] locParams, bool isPreProd, int depth, int maxDepth, out string loc)
	{
		loc = null;
		if (depth >= maxDepth + 1)
		{
			return false;
		}
		if (string.IsNullOrWhiteSpace(key))
		{
			loc = (isPreProd ? "<b>Invalid Key</b>" : string.Empty);
			return false;
		}
		depth++;
		foreach (IClientLocProvider nestedProvider in nestedProviders)
		{
			if (!nestedProvider.TryGetLocalizedTextForLanguage(key, overrideLangCode, locParams, out loc))
			{
				continue;
			}
			foreach (string subKey in LocalizationManagerUtilities.GetSubKeys(loc))
			{
				if (!TryGetLocalizedTextForLanguageInternal(nestedProviders, subKey, overrideLangCode, locParams, isPreProd, depth, maxDepth, out var loc2))
				{
					if (depth >= maxDepth + 1)
					{
						if (isPreProd)
						{
							loc2 = "Depth Limit Exceeded";
						}
					}
					else
					{
						loc2 = SubstituteEmptyKeyInPreProd(isPreProd, subKey, overrideLangCode);
					}
				}
				loc = LocalizationManagerUtilities.FillSubKey(loc, subKey, loc2);
			}
			return true;
		}
		return false;
	}

	private static string SubstituteEmptyKeyInPreProd(bool isPreProd, string key, string overrideLangCode)
	{
		if (!isPreProd)
		{
			return string.Empty;
		}
		return "<b>" + overrideLangCode + ": " + key + "</b>";
	}

	public string GetLocalizedTextForLanguage(string key, string overrideLangCode, params (string, string)[] locParams)
	{
		if (TryGetLocalizedTextForLanguage(key, overrideLangCode, locParams, out var loc))
		{
			return loc;
		}
		if (!string.IsNullOrEmpty(key))
		{
			_logger.Warn("[MTGA.Loc] Missing translation for " + key + " in " + overrideLangCode);
		}
		return SubstituteEmptyKeyInPreProd(IsPreProd, key, overrideLangCode);
	}

	public bool DoesContainTranslation(string key)
	{
		return GetKeys().Contains(key);
	}

	public bool IsDisposed()
	{
		return _disposed;
	}

	public IEnumerable<string> GetKeys()
	{
		return _nestedProviders.SelectMany((IClientLocProvider x) => x.GetKeys()).Distinct();
	}

	public void OpenConnection()
	{
		foreach (IClientLocProvider nestedProvider in _nestedProviders)
		{
			nestedProvider.OpenConnection();
		}
	}

	public void CloseConnection()
	{
		foreach (IClientLocProvider nestedProvider in _nestedProviders)
		{
			nestedProvider.CloseConnection();
		}
	}

	public void Dispose()
	{
		foreach (IClientLocProvider nestedProvider in _nestedProviders)
		{
			if (nestedProvider is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
		_nestedProviders.Clear();
		_disposed = true;
	}
}
