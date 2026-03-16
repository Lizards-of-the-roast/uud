using System.Collections.Generic;
using GreClient.Network;

namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public interface IMatchConfigProvider
{
	IReadOnlyList<string> GetAllDirectories();

	IReadOnlyList<MatchConfig> GetAllMatchConfigs(string subDirectory);

	MatchConfig GetMatchConfigByName(string subDirectory, string name);

	void SaveMatchConfig(string subDirectory, string name, MatchConfig matchConfig);

	bool RenameMatchConfig(string subDirectory, string oldName, string newName, MatchConfig matchConfig);
}
