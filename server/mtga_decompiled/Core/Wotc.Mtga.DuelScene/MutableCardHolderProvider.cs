using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public class MutableCardHolderProvider : ICardHolderProvider
{
	public readonly Dictionary<uint, ICardHolder> ZoneIdToCardHolder = new Dictionary<uint, ICardHolder>();

	public readonly Dictionary<(uint zoneId, uint playerId), ICardHolder> SubCardHolderMap = new Dictionary<(uint, uint), ICardHolder>();

	public readonly Dictionary<GREPlayerNum, Dictionary<CardHolderType, ICardHolder>> PlayerTypeMap = new Dictionary<GREPlayerNum, Dictionary<CardHolderType, ICardHolder>>();

	public readonly HashSet<CardHolderBase> AllCardHolders = new HashSet<CardHolderBase>();

	public ICardHolder GetCardHolderByZoneId(uint zoneId)
	{
		if (!ZoneIdToCardHolder.TryGetValue(zoneId, out var value))
		{
			return PlayerTypeMap[GREPlayerNum.Invalid][CardHolderType.Invalid];
		}
		return value;
	}

	public ICardHolder GetCardHolder(GREPlayerNum playerNum, CardHolderType cardHolderType)
	{
		if (PlayerTypeMap.TryGetValue(playerNum, out var value) && value.TryGetValue(cardHolderType, out var value2))
		{
			return value2;
		}
		if (PlayerTypeMap.TryGetValue(GREPlayerNum.Invalid, out value) && value.TryGetValue(cardHolderType, out value2))
		{
			return value2;
		}
		return null;
	}

	public bool TryGetCardHolder(GREPlayerNum playerNum, CardHolderType zoneType, out ICardHolder cardHolder)
	{
		cardHolder = GetCardHolder(playerNum, zoneType);
		return cardHolder != null;
	}
}
