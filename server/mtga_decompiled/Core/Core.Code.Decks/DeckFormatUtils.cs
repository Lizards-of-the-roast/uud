using System;
using Wizards.Arena.Enums.Card;
using Wizards.MDN.DeckManager;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Loc;

namespace Core.Code.Decks;

public static class DeckFormatUtils
{
	public static Client_Deck NewDeck(this DeckFormat deckFormat, IDeckNameProvider deckNameProvider)
	{
		Client_Deck client_Deck = new Client_Deck();
		client_Deck.UpdateWith(new Client_DeckSummary
		{
			DeckId = Guid.NewGuid(),
			Name = WrapperDeckUtilities.GetUniqueName(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckManager/DeckManager_Top_New"), deckNameProvider.GetAllDeckNames()),
			Format = deckFormat.FormatName
		});
		client_Deck.UpdateWith(new Client_DeckContents());
		return client_Deck;
	}

	public static string GetWarningText(SetAvailability availability)
	{
		return availability switch
		{
			SetAvailability.AlchemyNotStandard => Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/StandardUnavailable_Message"), 
			SetAvailability.StandardNotAlchemy => Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/AlchemyUnavailable_Message"), 
			SetAvailability.EternalOnly => Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/EternalOnly_Message"), 
			SetAvailability.HistoricOnly => Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/HistoricOnly_Message"), 
			SetAvailability.RotatingOutSoonStandard => Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/RotatingOutSoonStandard_Message"), 
			SetAvailability.RotatingOutSoonAlchemy => Languages.ActiveLocProvider.GetLocalizedText("MainNav/Store/Popups/SetRotation/RotatingOutSoonAlchemy_Message"), 
			_ => string.Empty, 
		};
	}
}
