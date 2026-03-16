using UnityEngine;

namespace Wotc.Mtga.DuelScene;

public class NullPlayerSpriteProvider : IPlayerSpriteProvider
{
	public static readonly IPlayerSpriteProvider Default = new NullPlayerSpriteProvider();

	public Sprite GetPlayerSprite(uint playerId)
	{
		return null;
	}
}
