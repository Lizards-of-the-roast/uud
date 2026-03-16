using UnityEngine;
using Wotc.Mtga.DuelScene.UI;

namespace Wotc.Mtga.DuelScene.PlayerNameViews;

public class NullPlayerNameViewBuilder : IPlayerNameViewBuilder
{
	public static readonly IPlayerNameViewBuilder Default = new NullPlayerNameViewBuilder();

	public PlayerName Create(GREPlayerNum playerNum, Transform root)
	{
		return null;
	}
}
