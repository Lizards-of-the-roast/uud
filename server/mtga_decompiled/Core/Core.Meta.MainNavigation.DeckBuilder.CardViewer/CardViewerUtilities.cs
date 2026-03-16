using System;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace Core.Meta.MainNavigation.DeckBuilder.CardViewer;

public static class CardViewerUtilities
{
	public static void OpenCardViewer(CardDatabase cardDatabase, CardPrintingData cardPrintingData, string skin, int quantityToCraft, Action<string> onSkinSelected, Action<Action> onNav)
	{
		uint artId = cardPrintingData.ArtId;
		if (AltPrintingUtilities.FindBasePrinting(cardPrintingData, cardDatabase.CardDataProvider, cardDatabase.AltPrintingProvider, out var basePrinting, WrapperController.Instance.Store.CardSkinCatalog))
		{
			artId = basePrinting.ArtId;
		}
		SceneLoader.GetSceneLoader().EnableCardViewerPopup(craftingMode: true, cardPrintingData.GrpId, skin, quantityToCraft, onSkinSelected, onNav, artId);
	}
}
