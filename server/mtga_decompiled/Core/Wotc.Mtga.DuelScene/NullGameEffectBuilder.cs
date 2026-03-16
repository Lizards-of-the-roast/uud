using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public class NullGameEffectBuilder : IGameEffectBuilder
{
	public static readonly IGameEffectBuilder Default = new NullGameEffectBuilder();

	public DuelScene_CDC Create(GameEffectType effectType, string key, ICardDataAdapter cardData)
	{
		return null;
	}

	public bool Destroy(string key)
	{
		return false;
	}
}
