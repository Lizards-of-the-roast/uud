using UnityEngine;
using Wotc.Mtga.DuelScene.UI;

namespace Wotc.Mtga.DuelScene.PlayerNameViews;

public interface IPlayerNameViewBuilder
{
	PlayerName Create(GREPlayerNum playerNum, Transform root);
}
