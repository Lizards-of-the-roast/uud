using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public class NullCardHolderManager : ICardHolderManager, ICardHolderProvider, ICardHolderController
{
	public static readonly ICardHolderManager Default = new NullCardHolderManager();

	private static readonly ICardHolderProvider Provider = NullCardHolderProvider.Default;

	private static readonly ICardHolderController Controller = NullCardHolderController.Default;

	public ICardHolder GetCardHolderByZoneId(uint zoneId)
	{
		return Provider.GetCardHolderByZoneId(zoneId);
	}

	public ICardHolder GetCardHolder(GREPlayerNum playerNum, CardHolderType zoneType)
	{
		return Provider.GetCardHolder(playerNum, zoneType);
	}

	public bool TryGetCardHolder(GREPlayerNum playerNum, CardHolderType zoneType, out ICardHolder cardHolder)
	{
		return Provider.TryGetCardHolder(playerNum, zoneType, out cardHolder);
	}

	public ICardHolder CreateCardHolder(MtgZone zone)
	{
		return Controller.CreateCardHolder(zone);
	}

	public ICardHolder CreateSubCardHolder(MtgZone zone, MtgPlayer player)
	{
		return Controller.CreateSubCardHolder(zone, player);
	}

	public bool DeleteCardHolder(uint zoneId)
	{
		return Controller.DeleteCardHolder(zoneId);
	}

	public bool DeleteSubCardHolder(uint zoneId, uint playerId)
	{
		return Controller.DeleteSubCardHolder(zoneId, playerId);
	}
}
