using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Scripting;
using Wotc.Mtga.Loc.CachingPatterns;

namespace Wotc.Mtga.Loc;

[CreateAssetMenu(fileName = "LocLibrary", menuName = "ScriptableObject/LocLibrary", order = 0)]
public class LocLibrary : ScriptableObject, IClientLocProvider
{
	public class LocEntry
	{
		public string Key;

		public List<LocText> Translations;

		[Preserve]
		public LocEntry()
		{
		}
	}

	public class LocText
	{
		public string Language;

		public string Translation;

		[Preserve]
		public LocText()
		{
		}
	}

	private static LocLibrary _instance;

	[SerializeField]
	private List<string> _fontPaths;

	[SerializeField]
	private TextAsset _textData;

	private readonly ICachingPattern<string, string> _cache = new DictionaryCache<string, string>(1000);

	private readonly HashSet<string> _keysCache = new HashSet<string>();

	public List<LocEntry> Texts;

	public static LocLibrary Instance => _instance ?? (_instance = Resources.Load<LocLibrary>("LocLibrary"));

	public List<string> FontPaths => _fontPaths;

	private void OnEnable()
	{
		Languages.LanguageChangedSignal.Listeners += OnLanguageChanged;
	}

	private void OnDisable()
	{
		Languages.LanguageChangedSignal.Listeners -= OnLanguageChanged;
	}

	public string GetFontResourcePath(string fontName)
	{
		string text = _fontPaths.Find((string x) => Path.GetFileNameWithoutExtension(x) == fontName);
		if (!string.IsNullOrEmpty(text))
		{
			int num = text.LastIndexOf("Resources/", StringComparison.Ordinal);
			if (num > 0)
			{
				text = text.Substring(num + "Resources/".Length);
			}
			text = text.Replace(Path.GetExtension(text), string.Empty);
		}
		return text;
	}

	private void OnLanguageChanged()
	{
		_cache.ClearCache();
	}

	public string GetLocalizedText(string key, params (string, string)[] locParams)
	{
		return GetLocalizedTextForLanguage(key, Languages.CurrentLanguage, locParams);
	}

	public bool TryGetLocalizedTextForLanguage(string key, string overrideLangCode, (string, string)[] locParams, out string loc)
	{
		if (Texts == null)
		{
			LoadTextData();
		}
		loc = null;
		if (string.IsNullOrEmpty(key))
		{
			return false;
		}
		bool flag = Languages.CurrentLanguage == overrideLangCode;
		if (!_cache.TryGetCached(key, out loc) || loc == null || !flag)
		{
			foreach (LocEntry text in Texts)
			{
				if (!(text.Key == key))
				{
					continue;
				}
				foreach (LocText translation in text.Translations)
				{
					if (translation.Language == overrideLangCode)
					{
						loc = translation.Translation;
						break;
					}
				}
				break;
			}
			if (loc != null && flag)
			{
				_cache.SetCached(key, loc);
			}
		}
		if (loc != null && locParams != null && locParams.Length != 0)
		{
			for (int i = 0; i < locParams.Length; i++)
			{
				(string, string) tuple = locParams[i];
				string item = tuple.Item1;
				string item2 = tuple.Item2;
				loc = loc.Replace("{" + item + "}", item2);
			}
		}
		return loc != null;
	}

	public string GetLocalizedTextForLanguage(string key, string overrideLangCode, params (string, string)[] locParams)
	{
		TryGetLocalizedTextForLanguage(key, overrideLangCode, locParams, out var loc);
		return loc;
	}

	public bool DoesContainTranslation(string key)
	{
		return GetKeys().Contains(key);
	}

	public bool IsDisposed()
	{
		return false;
	}

	public IEnumerable<string> GetKeys()
	{
		if (Texts == null)
		{
			LoadTextData();
		}
		if (_keysCache.Count == 0)
		{
			foreach (LocEntry text in Texts)
			{
				_keysCache.Add(text.Key);
			}
		}
		return _keysCache;
	}

	public void OpenConnection()
	{
	}

	public void CloseConnection()
	{
	}

	public void LoadTextData()
	{
		Texts = JsonConvert.DeserializeObject<List<LocEntry>>(_textData.text);
	}
}
