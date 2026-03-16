namespace Wotc.Mtga.DuelScene;

public class CardMovementController : ICardMovementController
{
	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly IAltCardHolderCalculator _altCardHolderCalculator;

	public CardMovementController(ICardHolderProvider cardHolderProvider, IAltCardHolderCalculator altCardHolderCalculator)
	{
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_altCardHolderCalculator = altCardHolderCalculator ?? NullAltCardHolderCalculator.Default;
	}

	public void MoveCard(DuelScene_CDC cardView, CardHolderType destination, bool forceAdd = false)
	{
		ICardHolder cardHolder = _cardHolderProvider.GetCardHolder(cardView.Model.ControllerNum, destination);
		MoveCard(cardView, cardHolder, forceAdd);
	}

	public void MoveCard(DuelScene_CDC cardView, uint zoneId, bool forceAdd = false)
	{
		MoveCard(cardView, _cardHolderProvider.GetCardHolderByZoneId(zoneId), forceAdd);
	}

	public void MoveCard(DuelScene_CDC cardView, ICardHolder destination, bool forceAdd = false)
	{
		_altCardHolderCalculator.TryGetAltCardHolder(cardView.Model, destination, out destination);
		if (destination == null)
		{
			if (cardView.CurrentCardHolder != null)
			{
				cardView.CurrentCardHolder.RemoveCard(cardView);
			}
		}
		else if (destination == cardView.CurrentCardHolder && destination.CardViews.Contains(cardView))
		{
			if (forceAdd)
			{
				cardView.CurrentCardHolder.SetCardAdded(cardView);
			}
			cardView.CurrentCardHolder.LayoutNow();
		}
		else
		{
			if (destination != cardView.CurrentCardHolder)
			{
				cardView.CurrentCardHolder.RemoveCard(cardView);
			}
			destination.AddCard(cardView);
		}
	}
}
