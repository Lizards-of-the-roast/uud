using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.DuelScene.UI;

namespace Wotc.Mtga.DuelScene.PlayerNameViews;

public class NullPlayerNameViewManager : IPlayerNameViewManager, IPlayerNameViewProvider, IPlayerNameViewController
{
	public static readonly IPlayerNameViewManager Default = new NullPlayerNameViewManager();

	private static readonly IPlayerNameViewProvider _provider = NullPlayerNameViewProvider.Default;

	private static readonly IPlayerNameViewController _controller = NullPlayerNameViewController.Default;

	public PlayerName CreatePlayerName(uint id, Transform localRoot, Transform oppRoot)
	{
		return _controller.CreatePlayerName(id, localRoot, oppRoot);
	}

	public PlayerName CreatePlayerNameNPE(uint id, Transform localRoot, Transform oppRoot)
	{
		return _controller.CreatePlayerNameNPE(id, localRoot, oppRoot);
	}

	public IReadOnlyList<PlayerNameViewData> GetAllPlayerNameDataList()
	{
		return _provider.GetAllPlayerNameDataList();
	}

	public PlayerName GetPlayerNameById(uint id)
	{
		return _provider.GetPlayerNameById(id);
	}

	public PlayerName GetPlayerNameByGrePlayerNum(GREPlayerNum playerNum)
	{
		return _provider.GetPlayerNameByGrePlayerNum(playerNum);
	}
}
