using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullCardHolderController : ICardHolderController
{
	public static readonly ICardHolderController Default = new NullCardHolderController();

	public ICardHolder CreateCardHolder(MtgZone zone)
	{
		return null;
	}

	public ICardHolder CreateSubCardHolder(MtgZone zone, MtgPlayer player)
	{
		return null;
	}

	public bool DeleteCardHolder(uint zoneId)
	{
		return false;
	}

	public bool DeleteSubCardHolder(uint zoneId, uint playerId)
	{
		return false;
	}
}
