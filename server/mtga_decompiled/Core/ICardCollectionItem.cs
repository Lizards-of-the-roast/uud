using GreClient.CardData;

public interface ICardCollectionItem
{
	CardData Card { get; }

	int Quantity { get; }
}
