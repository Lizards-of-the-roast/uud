using System;
using System.Collections.Generic;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public class NullMatchConfigProvider : IMatchConfigProvider
{
	public static readonly IMatchConfigProvider Default = new NullMatchConfigProvider();

	public IReadOnlyList<string> GetAllDirectories()
	{
		return Array.Empty<string>();
	}

	public IReadOnlyList<MatchConfig> GetAllMatchConfigs(string subDirectory)
	{
		return Array.Empty<MatchConfig>();
	}

	public MatchConfig GetMatchConfigByName(string subDirectory, string name)
	{
		return default(MatchConfig);
	}

	public void SaveMatchConfig(string subDirectory, string name, MatchConfig matchConfig)
	{
	}

	public bool RenameMatchConfig(string subDirectory, string oldName, string newName, MatchConfig matchConfig)
	{
		return false;
	}
}
