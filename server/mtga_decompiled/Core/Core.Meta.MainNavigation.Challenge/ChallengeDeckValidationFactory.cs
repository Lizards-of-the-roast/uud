using Core.Shared.Code.Providers;
using SharedClientCore.SharedClientCore.Code.Providers;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Challenge;

public class ChallengeDeckValidationFactory
{
	public static ChallengeDeckValidation Create()
	{
		DeckDataProvider deckDataProvider = Pantry.Get<DeckDataProvider>();
		CosmeticsProvider cosmeticProvider = Pantry.Get<CosmeticsProvider>();
		FormatManager formatManager = Pantry.Get<FormatManager>();
		InventoryManager inventoryManager = Pantry.Get<InventoryManager>();
		ITitleCountManager titleCountManager = Pantry.Get<ITitleCountManager>();
		CardDatabase cardDatabase = Pantry.Get<CardDatabase>();
		IEmergencyCardBansProvider emergencyCardBansProvider = Pantry.Get<IEmergencyCardBansProvider>();
		ISetMetadataProvider setMetadataProvider = Pantry.Get<ISetMetadataProvider>();
		DesignerMetadataProvider designerMetadataProvider = Pantry.Get<DesignerMetadataProvider>();
		return new ChallengeDeckValidation(deckDataProvider, cosmeticProvider, formatManager, inventoryManager, titleCountManager, cardDatabase, emergencyCardBansProvider, setMetadataProvider, designerMetadataProvider);
	}
}
