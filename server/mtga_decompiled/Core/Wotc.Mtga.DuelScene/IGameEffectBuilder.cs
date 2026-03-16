using GreClient.CardData;

namespace Wotc.Mtga.DuelScene;

public interface IGameEffectBuilder
{
	DuelScene_CDC Create(GameEffectType effectType, string key, ICardDataAdapter cardData);

	bool Destroy(string key);
}
