using System;
using Core.Shared.Code.Providers;
using SharedClientCore.SharedClientCore.Code.Decks.DeckValidation;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Arena.DeckValidation.Client;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Challenge;

public class ChallengeDeckValidation : IChallengeDeckValidation
{
	private DeckDataProvider _deckDataProvider;

	private CosmeticsProvider _cosmeticsProvider;

	private IFormatManager _formatManager;

	private InventoryManager _inventoryManager;

	private ITitleCountManager _titleCountManager;

	private CardDatabase _cardDatabase;

	private IEmergencyCardBansProvider _emergencyCardBansProvider;

	private ISetMetadataProvider _setMetadataProvider;

	private IDesignerMetadataProvider _designerMetadataProvider;

	public ChallengeDeckValidation(DeckDataProvider deckDataProvider, CosmeticsProvider cosmeticProvider, IFormatManager formatManager, InventoryManager inventoryManager, ITitleCountManager titleCountManager, CardDatabase cardDatabase, IEmergencyCardBansProvider emergencyCardBansProvider, ISetMetadataProvider setMetadataProvider, IDesignerMetadataProvider designerMetadataProvider)
	{
		_deckDataProvider = deckDataProvider;
		_cosmeticsProvider = cosmeticProvider;
		_formatManager = formatManager;
		_inventoryManager = inventoryManager;
		_titleCountManager = titleCountManager;
		_cardDatabase = cardDatabase;
		_emergencyCardBansProvider = emergencyCardBansProvider;
		_setMetadataProvider = setMetadataProvider;
		_designerMetadataProvider = designerMetadataProvider;
	}

	public ClientSideDeckValidationResult ValidateDeck(Guid deckId, string format)
	{
		return DeckValidationHelper.CalculateIsDeckLegalAndOwned(_formatManager.GetSafeFormat(format), _deckDataProvider.GetDeckForId(deckId), _inventoryManager.Cards, _titleCountManager.OwnedTitleCounts, _cardDatabase, _emergencyCardBansProvider, _setMetadataProvider, _cosmeticsProvider, _designerMetadataProvider, null);
	}
}
