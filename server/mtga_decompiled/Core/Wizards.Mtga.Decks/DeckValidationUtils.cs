using System;
using System.Linq;
using Core.Shared.Code.Providers;
using SharedClientCore.SharedClientCore.Code.Decks.DeckValidation;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Arena.DeckValidation.Client;
using Wizards.Arena.DeckValidation.Core.Models;
using Wizards.Arena.Enums.Format;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace Wizards.Mtga.Decks;

public static class DeckValidationUtils
{
	public static int DECK_NAME_MAX_LENGTH = 64;

	public static DeckDisplayInfo CalculateDisplayInfo(Client_Deck deck, IColorChallengeStrategy colorChallenge, DeckFormat format, InventoryManager inventoryManager, ITitleCountManager titleCountManager, CardDatabase cardDatabase, IClientLocProvider locMan, IEmergencyCardBansProvider emergencyCardBansProvider, ISetMetadataProvider setMetadataProvider, CosmeticsProvider cosmeticsProvider, DesignerMetadataProvider designerMetadataProvider)
	{
		ClientSideDeckValidationResult clientSideDeckValidationResult = DeckValidationHelper.CalculateIsDeckLegalAndOwned(format, deck, inventoryManager.Cards, titleCountManager.OwnedTitleCounts, cardDatabase, emergencyCardBansProvider, setMetadataProvider, cosmeticsProvider, designerMetadataProvider, null, (FormatType.None, ContextType.CompanionValidity));
		string colorChallengeEventLock = null;
		if (!string.IsNullOrWhiteSpace(deck.Summary.Description))
		{
			IColorChallengeTrack colorChallengeTrack = colorChallenge.Tracks.Values.FirstOrDefault((IColorChallengeTrack e) => e.DeckSummary?.Description == deck.Summary.Description);
			if (colorChallengeTrack != null && !colorChallengeTrack.Completed)
			{
				colorChallengeEventLock = colorChallengeTrack.Name;
			}
		}
		return new DeckDisplayInfo(displayState: (!clientSideDeckValidationResult.IsValid) ? ((clientSideDeckValidationResult.NumberBannedCards + clientSideDeckValidationResult.NumberEmergencyBannedCards + clientSideDeckValidationResult.NumberNonFormatCard + clientSideDeckValidationResult.CardTitlesOverRestrictedListQuota.Count > 0) ? DeckDisplayInfo.DeckDisplayState.Invalid : ((!clientSideDeckValidationResult.HasUnOwnedCards) ? DeckDisplayInfo.DeckDisplayState.Malformed : (clientSideDeckValidationResult.IsCraftable(inventoryManager.Inventory.CombinedWildcardInventory()) ? DeckDisplayInfo.DeckDisplayState.Craftable : DeckDisplayInfo.DeckDisplayState.Uncraftable))) : DeckDisplayInfo.DeckDisplayState.Valid, deck: deck, validationResult: clientSideDeckValidationResult, tooltipText: locMan.GetLocalizedText("DuelScene/Browsers/Select_Deck_Tooltip"), useHistoricLabel: format.IsHistoric, colorChallengeEventLock: colorChallengeEventLock);
	}

	public static bool ValidateDeckNameWithSystemMessages(Guid deckID, string deckName, Action respondToFailure, Action<string> tryResave, bool isConstructed)
	{
		if (DeckNameIsTooShort(deckName))
		{
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_DeckNameIsRequired"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageContent_DeckNameIsRequired"), showCancel: false, respondToFailure);
			return false;
		}
		if (DeckNameIsTooLong(deckName, DECK_NAME_MAX_LENGTH))
		{
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_DeckNameIsTooLong"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageContent_DeckNameIsTooLong"), showCancel: true, delegate
			{
				string obj = deckName.Substring(0, DECK_NAME_MAX_LENGTH);
				tryResave?.Invoke(obj);
			}, respondToFailure);
			return false;
		}
		if (isConstructed && DeckNameIsNotUnique(deckName, deckID, WrapperController.Instance.DecksManager))
		{
			SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageTitle_DeckNameCannotBeDup"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_MessageContent_DeckNameCannotBeDup"), showCancel: false, delegate
			{
				string uniqueName = WrapperDeckUtilities.GetUniqueName(deckName, WrapperController.Instance.DecksManager.GetAllDeckNames());
				tryResave?.Invoke(uniqueName);
			});
			return false;
		}
		return true;
	}

	public static bool DeckNameIsTooShort(string deckName)
	{
		if (string.IsNullOrWhiteSpace(deckName))
		{
			return true;
		}
		return false;
	}

	public static bool DeckNameIsTooLong(string deckName, int deckLength)
	{
		if (deckName.Length > deckLength)
		{
			return true;
		}
		return false;
	}

	public static bool DeckNameIsNotUnique(string deckName, Guid deckID, DecksManager decksManager)
	{
		if (decksManager.DeckNameAlreadyExists(deckID, deckName))
		{
			return true;
		}
		return false;
	}
}
