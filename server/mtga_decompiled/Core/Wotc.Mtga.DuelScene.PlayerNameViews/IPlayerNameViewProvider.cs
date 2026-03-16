using System.Collections.Generic;
using Wotc.Mtga.DuelScene.UI;

namespace Wotc.Mtga.DuelScene.PlayerNameViews;

public interface IPlayerNameViewProvider
{
	PlayerName GetPlayerNameById(uint id);

	IReadOnlyList<PlayerNameViewData> GetAllPlayerNameDataList();

	PlayerName GetPlayerNameByGrePlayerNum(GREPlayerNum playerNum);
}
