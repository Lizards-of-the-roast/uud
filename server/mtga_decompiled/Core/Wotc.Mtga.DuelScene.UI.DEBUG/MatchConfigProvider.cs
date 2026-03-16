using System;
using System.Collections.Generic;
using System.IO;
using GreClient.Network;
using Newtonsoft.Json;
using Wotc.Mtga.DuelScene.UI.DEBUG.JsonConversion;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class MatchConfigProvider : IMatchConfigProvider
{
	private static readonly JsonConverter[] _converters = new JsonConverter[5]
	{
		new MatchConfigConverter(TeamConfig.Default()),
		new TeamConfigConverter(),
		new PlayerConfigConverter(),
		new DeckConfigConverter(),
		new RankConfigConverter()
	};

	private readonly string _directoryPath;

	private readonly MatchConfig _defaultConfig;

	private List<string> _allDirectories;

	private Dictionary<string, List<MatchConfig>> _allMatchConfigsCache;

	private Dictionary<string, List<MatchConfig>> _allMatchConfigs => _allMatchConfigsCache ?? (_allMatchConfigsCache = LoadAllMatchConfigs());

	public MatchConfigProvider(string directoryPath, MatchConfig defaultConfig)
	{
		_directoryPath = directoryPath;
		_defaultConfig = defaultConfig;
	}

	public IReadOnlyList<string> GetAllDirectories()
	{
		return _allDirectories ?? (_allDirectories = new List<string>(_allMatchConfigs.Keys));
	}

	public IReadOnlyList<MatchConfig> GetAllMatchConfigs(string subDirectory)
	{
		if (!_allMatchConfigs.TryGetValue(subDirectory, out var value))
		{
			return Array.Empty<MatchConfig>();
		}
		return value;
	}

	public MatchConfig GetMatchConfigByName(string subDirectory, string name)
	{
		if (_allMatchConfigs.TryGetValue(subDirectory, out var value) && value.Count > 0)
		{
			foreach (MatchConfig item in value)
			{
				if (item.Name == name)
				{
					return item;
				}
			}
			return value[0];
		}
		return _defaultConfig;
	}

	public void SaveMatchConfig(string subDirectory, string name, MatchConfig matchConfig)
	{
		bool flag = true;
		for (int i = 0; i < GetAllMatchConfigs(subDirectory).Count; i++)
		{
			if (_allMatchConfigs[subDirectory][i].Name == name)
			{
				flag = false;
				_allMatchConfigs[subDirectory][i] = matchConfig;
				break;
			}
		}
		if (flag)
		{
			_allMatchConfigs[subDirectory].Add(matchConfig);
		}
		SerializeMatchConfig(GetDirectoryPath(subDirectory), name, matchConfig);
	}

	private void SerializeMatchConfig(string path, string name, MatchConfig matchConfig)
	{
		string contents = JsonConvert.SerializeObject(matchConfig, Formatting.Indented, _converters);
		File.WriteAllText(Path.Combine(path, name + ".json"), contents);
	}

	public bool RenameMatchConfig(string subDirectory, string oldName, string newName, MatchConfig matchConfig)
	{
		IReadOnlyList<MatchConfig> allMatchConfigs = GetAllMatchConfigs(subDirectory);
		foreach (MatchConfig item in allMatchConfigs)
		{
			if (item.Name == newName)
			{
				return false;
			}
		}
		bool flag = false;
		for (int i = 0; i < allMatchConfigs.Count; i++)
		{
			if (_allMatchConfigs[subDirectory][i].Name == oldName)
			{
				_allMatchConfigs[subDirectory][i] = matchConfig;
				flag = true;
				break;
			}
		}
		if (flag)
		{
			File.Delete(Path.Combine(GetDirectoryPath(subDirectory), oldName + ".json"));
			SaveMatchConfig(subDirectory, newName, matchConfig);
		}
		return flag;
	}

	private Dictionary<string, List<MatchConfig>> LoadAllMatchConfigs()
	{
		Dictionary<string, List<MatchConfig>> dictionary = new Dictionary<string, List<MatchConfig>>();
		foreach (string item in LoadDirectories())
		{
			List<MatchConfig> list = new List<MatchConfig>(LoadAllMatchConfigs(item));
			if (list.Count > 0)
			{
				dictionary[item] = list;
			}
		}
		if (dictionary.Count == 0)
		{
			Directory.CreateDirectory(_directoryPath);
			SerializeMatchConfig(_directoryPath, "Default", _defaultConfig);
			dictionary[string.Empty] = new List<MatchConfig> { _defaultConfig };
		}
		return dictionary;
	}

	private IEnumerable<string> LoadDirectories()
	{
		if (Directory.Exists(_directoryPath))
		{
			yield return string.Empty;
			DirectoryInfo directoryInfo = new DirectoryInfo(_directoryPath);
			DirectoryInfo[] directories = directoryInfo.GetDirectories();
			foreach (DirectoryInfo directoryInfo2 in directories)
			{
				yield return directoryInfo2.Name;
			}
		}
	}

	private IEnumerable<MatchConfig> LoadAllMatchConfigs(string subDirectory)
	{
		if (!Directory.Exists(GetDirectoryPath(subDirectory)))
		{
			yield break;
		}
		string[] files = Directory.GetFiles(GetDirectoryPath(subDirectory), "*.json");
		string[] array = files;
		foreach (string path in array)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(path);
			MatchConfig matchConfig = JsonConvert.DeserializeObject<MatchConfig>(File.ReadAllText(path), _converters);
			matchConfig = ApplyVersionMigrations(subDirectory, fileNameWithoutExtension, matchConfig);
			MatchConfig matchConfig2;
			if (!(fileNameWithoutExtension != matchConfig.Name))
			{
				matchConfig2 = matchConfig;
			}
			else
			{
				MatchConfig baseConfig = matchConfig;
				string name = fileNameWithoutExtension;
				matchConfig2 = new MatchConfig(baseConfig, null, name);
			}
			yield return matchConfig2;
		}
	}

	private MatchConfig ApplyVersionMigrations(string subDirectory, string fileName, MatchConfig matchConfig)
	{
		MatchConfig matchConfig2 = matchConfig;
		if (matchConfig2.Version == 0)
		{
			string battlefieldSelection = ((matchConfig2.BattlefieldSelection == "TEST") ? "LGS" : matchConfig2.BattlefieldSelection);
			matchConfig2 = new MatchConfig(matchConfig2, 1u, null, battlefieldSelection);
			SerializeMatchConfig(GetDirectoryPath(subDirectory), fileName, matchConfig2);
		}
		return matchConfig2;
	}

	private string GetDirectoryPath(string subDirectory)
	{
		if (!string.IsNullOrEmpty(subDirectory))
		{
			return Path.Combine(_directoryPath, subDirectory);
		}
		return _directoryPath;
	}
}
