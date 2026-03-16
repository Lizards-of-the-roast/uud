namespace Wotc.Mtga.DuelScene;

public interface ICardHolderProvider
{
	ICardHolder GetCardHolderByZoneId(uint zoneId);

	ICardHolder GetCardHolder(GREPlayerNum playerNum, CardHolderType zoneType);

	bool TryGetCardHolder(GREPlayerNum playerNum, CardHolderType zoneType, out ICardHolder cardHolder);

	bool TryGetCardHolderByZoneId(uint zoneId, out ICardHolder cardHolder)
	{
		cardHolder = GetCardHolderByZoneId(zoneId);
		return cardHolder != null;
	}

	T GetCardHolder<T>(GREPlayerNum playerType, CardHolderType cardHolderType) where T : ICardHolder
	{
		return (T)GetCardHolder(playerType, cardHolderType);
	}

	T GetCardHolder<T>(uint zoneId) where T : ICardHolder
	{
		return (T)GetCardHolderByZoneId(zoneId);
	}

	bool TryGetCardHolder<T>(GREPlayerNum playerType, CardHolderType cardHolderType, out T result) where T : ICardHolder
	{
		result = GetCardHolder<T>(playerType, cardHolderType);
		return result != null;
	}

	bool TryGetCardHolder<T>(uint zoneId, out T result) where T : ICardHolder
	{
		result = GetCardHolder<T>(zoneId);
		return result != null;
	}
}
