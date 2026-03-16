namespace Wotc.Mtga.DuelScene;

public class NullCardHolderProvider : ICardHolderProvider
{
	public static readonly ICardHolderProvider Default = new NullCardHolderProvider();

	public ICardHolder GetCardHolder(GREPlayerNum playerNum, CardHolderType zoneType)
	{
		return null;
	}

	public bool TryGetCardHolder(GREPlayerNum playerNum, CardHolderType zoneType, out ICardHolder cardHolder)
	{
		cardHolder = null;
		return false;
	}

	public ICardHolder GetCardHolderByZoneId(uint zoneId)
	{
		return null;
	}
}
