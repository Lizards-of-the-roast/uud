using UnityEngine;
using Wotc.Mtga.DuelScene.UI;

namespace Wotc.Mtga.DuelScene.PlayerNameViews;

public interface IPlayerNameViewController
{
	PlayerName CreatePlayerName(uint id, Transform localRoot, Transform oppRoot);

	PlayerName CreatePlayerNameNPE(uint id, Transform localRoot, Transform oppRoot);
}
