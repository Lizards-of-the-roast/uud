using System;
using System.Collections.Generic;
using System.IO;
using Core.Code.ClientFeatureToggle;
using Newtonsoft.Json;
using Wizards.Mtga.Platforms;

namespace Wizards.Mtga;

public class OverridesConfiguration : IClientFeatureToggleCollection
{
	public const string DefaultConfigurationFilename = "overrides.conf";

	public const string DebugKey = "debug";

	[JsonProperty("features")]
	private Dictionary<string, bool> _overrides;

	private static OverridesConfiguration _localConfig;

	[JsonIgnore]
	public IReadOnlyDictionary<string, bool> AllOverrides => _overrides;

	public static string ConfigPath => PlatformContext.GetStorageContext().GetOverrideConfigurationPath();

	public static OverridesConfiguration Local
	{
		get
		{
			if (_localConfig == null)
			{
				try
				{
					_localConfig = LoadOrCreateConfiguration(ConfigPath);
				}
				catch (JsonSerializationException e)
				{
					SimpleLog.LogException(e);
					_localConfig = new OverridesConfiguration();
				}
			}
			return _localConfig;
		}
		set
		{
			try
			{
				WriteConfiguration(ConfigPath, value);
				_localConfig = value;
			}
			catch (Exception e)
			{
				SimpleLog.LogException(e);
			}
		}
	}

	public bool GetFeatureToggleValue(string name)
	{
		if (_overrides == null)
		{
			return false;
		}
		bool value;
		return _overrides.TryGetValue(name, out value) && value;
	}

	public bool HasFeatureToggleValue(string name)
	{
		return _overrides.ContainsKey(name);
	}

	public void SetFeatureToggleValue(string name, bool value)
	{
		_overrides[name] = value;
	}

	public void RemoveFeatureToggleValue(string name)
	{
		_overrides.Remove(name);
	}

	private static OverridesConfiguration LoadOrCreateConfiguration(string path)
	{
		if (!new FileInfo(path).Exists)
		{
			return new OverridesConfiguration
			{
				_overrides = new Dictionary<string, bool>()
			};
		}
		SimpleLog.LogWarningForRelease("[config] Using configuraiton override file " + path);
		using JsonTextReader reader = new JsonTextReader(new StreamReader(path));
		return new JsonSerializer().Deserialize<OverridesConfiguration>(reader);
	}

	private static void WriteConfiguration(string path, OverridesConfiguration config)
	{
		using JsonTextWriter jsonWriter = new JsonTextWriter(new StreamWriter(path));
		JsonSerializer.Create(new JsonSerializerSettings
		{
			Formatting = Formatting.Indented
		}).Serialize(jsonWriter, config);
	}
}
