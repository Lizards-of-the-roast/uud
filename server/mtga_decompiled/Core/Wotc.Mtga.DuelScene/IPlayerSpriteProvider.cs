using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public interface IPlayerSpriteProvider
{
	Sprite GetPlayerSprite(uint playerId);
}
