using GreClient.CardData;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.Hangers.AbilityHangers;

public class ConfigProviderTranslator : IConfigProviderTranslator
{
	private readonly IHangerConfigProvider _dungeonProvider = new NullConfigProvider();

	public IHangerConfigProvider Translate(ICardDataAdapter cardData)
	{
		if (IsDungeonInCommand(cardData))
		{
			return _dungeonProvider;
		}
		return null;
	}

	public static bool IsDungeonInCommand(ICardDataAdapter cardData)
	{
		if (cardData == null)
		{
			return false;
		}
		if (cardData.Zone == null)
		{
			return false;
		}
		if (cardData.ZoneType == ZoneType.Command)
		{
			return cardData.CardTypes.Contains(CardType.Dungeon);
		}
		return false;
	}
}
