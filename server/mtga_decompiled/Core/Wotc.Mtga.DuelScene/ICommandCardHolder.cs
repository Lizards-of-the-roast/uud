using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene;

public interface ICommandCardHolder : ICardHolder
{
	void AddSubCardHolderForPlayer(ICardHolder cardHolder, uint playerId);

	ICardHolder GetSubCardHolderForPlayer(uint playerId);

	bool RemoveSubCardHolderForPlayer(uint playerId);

	IEnumerable<ICardHolder> GetAllSubCardHolders();
}
