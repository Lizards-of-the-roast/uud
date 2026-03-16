using System.Collections.Generic;
using Wotc.Mtga.DuelScene.UI;

namespace Wotc.Mtga.DuelScene.PlayerNameViews;

public class NullPlayerNameViewProvider : IPlayerNameViewProvider
{
	public static readonly IPlayerNameViewProvider Default = new NullPlayerNameViewProvider();

	public PlayerName GetPlayerNameById(uint id)
	{
		return null;
	}

	public IReadOnlyList<PlayerNameViewData> GetAllPlayerNameDataList()
	{
		return null;
	}

	public PlayerName GetPlayerNameByGrePlayerNum(GREPlayerNum playerNum)
	{
		return null;
	}
}
