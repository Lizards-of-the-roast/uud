using GreClient.CardData;

namespace Wizards.MDN.Store;

public readonly struct CardDataForTile : ICardCollectionItem
{
	public readonly bool IsArtStyle;

	public CardData Card { get; }

	public int Quantity { get; }

	public CardDataForTile(CardData card, uint quantity, bool isArtStyle)
	{
		Card = card;
		Quantity = (int)quantity;
		IsArtStyle = isArtStyle;
	}

	public void Deconstruct(out CardData card, out uint quantity, out bool isArtStyle)
	{
		card = Card;
		quantity = (uint)Quantity;
		isArtStyle = IsArtStyle;
	}
}
