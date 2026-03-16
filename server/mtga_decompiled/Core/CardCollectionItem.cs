using GreClient.CardData;

public class CardCollectionItem : ICardCollectionItem
{
	public CardData Card { get; private set; }

	public int Quantity { get; set; }

	public CardCollectionItem(CardData card, int quantity)
	{
		Card = card;
		Quantity = quantity;
	}
}
