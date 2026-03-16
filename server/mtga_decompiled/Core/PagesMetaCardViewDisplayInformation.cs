using GreClient.CardData;

public class PagesMetaCardViewDisplayInformation
{
	public bool UseCustomAutoLandsToggleObject;

	public CardPrintingData Card;

	public uint RemainingTitleCount;

	public uint UsedPrintingCount;

	public uint AvailablePrintingCount;

	public uint UsedTitleCount;

	public uint AvailableTitleCount;

	public uint UnownedPrintingCount;

	public int Max;

	public PagesMetaCardView.Tint Tint;

	public PagesMetaCardView.QuantityDisplayStyle QuantityStyle;

	public PagesMetaCardView.PipsDisplayStyle PipsStyle;

	public PagesMetaCardView.ExpandedDisplayStyle ExpandedStyle;

	public string Skin;

	public bool UseNewTag;

	public bool UseFactionTag;

	public string FactionTag;

	public bool PoolContainsNewCards;

	public bool IsCollapsing;

	public bool IsSideboardingOrLimited;

	public PagesMetaCardViewDisplayInformation GetCopy()
	{
		return (PagesMetaCardViewDisplayInformation)MemberwiseClone();
	}
}
