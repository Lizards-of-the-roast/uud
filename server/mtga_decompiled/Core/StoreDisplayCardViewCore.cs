using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

public class StoreDisplayCardViewCore
{
	public static bool IsSkuArtStyleForCard(string sku, CardData data, ICardDatabaseAdapter cardDatabase)
	{
		if (string.IsNullOrEmpty(sku))
		{
			return false;
		}
		string[] array = sku.Split('_');
		if (array.Length == 2)
		{
			string skinCode = array[1];
			if (uint.TryParse(array[0], out var result))
			{
				if (data.Printing.ArtId == result)
				{
					return true;
				}
				if (AltPrintingUtilities.FindAlternatePrinting(data.GrpId, skinCode, cardDatabase.CardDataProvider, cardDatabase.AltPrintingProvider, out var altPrinting, out var _) && altPrinting != null && altPrinting.ArtId == result)
				{
					return true;
				}
				if (AltPrintingUtilities.FindBasePrinting(data.Printing, cardDatabase.CardDataProvider, cardDatabase.AltPrintingProvider, out var basePrinting) && basePrinting != null && basePrinting.ArtId == result)
				{
					return true;
				}
			}
		}
		return false;
	}
}
