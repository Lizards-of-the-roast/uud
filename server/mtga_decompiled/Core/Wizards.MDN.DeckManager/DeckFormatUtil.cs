using System;
using System.Collections.Generic;
using Core.Shared.Code.Providers;
using SharedClientCore.SharedClientCore.Code.Decks.DeckValidation;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Providers;

namespace Wizards.MDN.DeckManager;

public class DeckFormatUtil
{
	public static string GetBestFormat(Client_Deck clientDeck, FormatManager formatManager, CardDatabase cardDb)
	{
		List<(string, Func<DeckInfo, bool>)> obj = new List<(string, Func<DeckInfo, bool>)>
		{
			("Brawl", (DeckInfo deck) => deck.commandZone.Count > 0),
			("HistoricBrawl", (DeckInfo deck) => deck.commandZone.Count > 0),
			("DirectGameBrawl", (DeckInfo deck) => deck.commandZone.Count > 0),
			("DirectGameBrawlRebalanced", (DeckInfo deck) => deck.commandZone.Count > 0),
			("Singleton", null),
			("Pauper", null),
			("Alchemy", (DeckInfo deck) => deck.sideboard.Count <= 7),
			("TraditionalAlchemy", (DeckInfo deck) => deck.sideboard.Count > 7),
			("Standard", (DeckInfo deck) => deck.sideboard.Count <= 7),
			("TraditionalStandard", (DeckInfo deck) => deck.sideboard.Count > 7),
			("Historic", (DeckInfo deck) => deck.sideboard.Count <= 7),
			("TraditionalHistoric", (DeckInfo deck) => deck.sideboard.Count > 7),
			("Explorer", (DeckInfo deck) => deck.sideboard.Count <= 7),
			("TraditionalExplorer", (DeckInfo deck) => deck.sideboard.Count > 7),
			("Timeless", (DeckInfo deck) => deck.sideboard.Count <= 7),
			("TraditionalTimeless", (DeckInfo deck) => deck.sideboard.Count > 7),
			("DirectGameLimited", null),
			("DirectGameLimitedRebalanced", null)
		};
		DeckInfo deckInfo = DeckServiceWrapperHelpers.ToAzureModel(clientDeck);
		foreach (var (text, func) in obj)
		{
			if ((func == null || func(deckInfo)) && ValidForFormat(text, deckInfo, formatManager, cardDb))
			{
				return text;
			}
		}
		foreach (var (result, func2) in new List<(string, Func<DeckInfo, bool>)>
		{
			("Brawl", (DeckInfo di) => di.commandZone.Count > 0 && di.mainDeck.Count <= 60),
			("HistoricBrawl", (DeckInfo di) => di.commandZone.Count > 0)
		})
		{
			if (func2 != null && func2(deckInfo))
			{
				return result;
			}
		}
		return "Historic";
	}

	private static bool ValidForFormat(string formatName, DeckInfo deck, FormatManager formatManager, CardDatabase cardDb)
	{
		return DeckValidationHelper.CalculateIsDeckLegal(formatManager.GetSafeFormat(formatName), deck, cardDb, Pantry.Get<IEmergencyCardBansProvider>(), Pantry.Get<ISetMetadataProvider>(), Pantry.Get<CosmeticsProvider>(), Pantry.Get<DesignerMetadataProvider>()).IsValid;
	}

	public static bool RemoveUnpublishedCards(Client_Deck deck, ISetMetadataProvider setMetadataProvider, CardDatabase cardDb)
	{
		int num = 0;
		foreach (KeyValuePair<EDeckPile, List<Client_DeckCard>> pile in deck.Contents.Piles)
		{
			num += pile.Value.RemoveAll((Client_DeckCard card) => !setMetadataProvider.IsSetPublished(cardDb.CardDataProvider.GetCardPrintingById(card.Id)?.ExpansionCode));
		}
		return num > 0;
	}
}
