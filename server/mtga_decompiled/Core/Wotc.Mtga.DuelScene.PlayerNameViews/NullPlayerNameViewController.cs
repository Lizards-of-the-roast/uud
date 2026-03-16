using UnityEngine;
using Wotc.Mtga.DuelScene.UI;

namespace Wotc.Mtga.DuelScene.PlayerNameViews;

public class NullPlayerNameViewController : IPlayerNameViewController
{
	public static readonly IPlayerNameViewController Default = new NullPlayerNameViewController();

	public PlayerName CreatePlayerName(uint id, Transform localRoot, Transform oppRoot)
	{
		return null;
	}

	public PlayerName CreatePlayerNameNPE(uint id, Transform localRoot, Transform oppRoot)
	{
		return null;
	}
}
