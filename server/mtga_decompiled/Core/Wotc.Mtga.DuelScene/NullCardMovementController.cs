namespace Wotc.Mtga.DuelScene;

public class NullCardMovementController : ICardMovementController
{
	public static readonly ICardMovementController Default = new NullCardMovementController();

	public void MoveCard(DuelScene_CDC cardView, CardHolderType destination, bool forceAdd = false)
	{
	}

	public void MoveCard(DuelScene_CDC cardView, ICardHolder destination, bool forceAdd = false)
	{
	}

	public void MoveCard(DuelScene_CDC cardView, uint zoneId, bool forceAdd = false)
	{
	}
}
