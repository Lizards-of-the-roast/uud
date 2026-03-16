using GreClient.Rules;

namespace Wotc.Mtga.DuelScene;

public interface ICardMovementController
{
	void MoveCard(DuelScene_CDC cardView, CardHolderType destination, bool forceAdd = false);

	void MoveCard(DuelScene_CDC cardView, ICardHolder destination, bool forceAdd = false);

	void MoveCard(DuelScene_CDC cardView, uint zoneId, bool forceAdd = false);

	void MoveCard(DuelScene_CDC cardView, MtgZone zone, bool forceAdd = false)
	{
		MoveCard(cardView, zone.Id, forceAdd);
	}
}
