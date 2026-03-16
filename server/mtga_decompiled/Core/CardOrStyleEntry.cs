public readonly struct CardOrStyleEntry
{
	public readonly CardPrintingQuantity PrintingQuantity;

	public readonly StyleInformation? StyleInformation;

	public CardOrStyleEntry(CardPrintingQuantity printingQuantity, in StyleInformation? styleInformation)
	{
		PrintingQuantity = printingQuantity;
		StyleInformation = styleInformation;
	}
}
