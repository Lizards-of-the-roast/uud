using Wizards.Unification.Models.Mercantile;

public readonly struct CardBackSelectorDisplayData
{
	public readonly string Name;

	public readonly bool Collected;

	public readonly string ListingId;

	public readonly EStoreSection StoreSection;

	public CardBackSelectorDisplayData(string cardBack, bool collected, EStoreSection storeSection = EStoreSection.None, string listingId = null)
	{
		Name = cardBack;
		Collected = collected;
		StoreSection = storeSection;
		ListingId = listingId;
	}
}
