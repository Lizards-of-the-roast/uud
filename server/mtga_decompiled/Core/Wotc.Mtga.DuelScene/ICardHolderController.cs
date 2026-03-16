using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface ICardHolderController
{
	ICardHolder CreateCardHolder(MtgZone zone);

	ICardHolder CreateSubCardHolder(MtgZone zone, MtgPlayer player);

	bool DeleteCardHolder(uint zoneId);

	bool DeleteSubCardHolder(uint zoneId, uint playerId);
}
