using System;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.DeckBuilder.Utilities;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using Wizards.Arena.Enums.Cosmetic;
using Wizards.Mtga;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga;
using Wotc.Mtga.Cards.ArtCrops;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Providers;

namespace Core.Code.Decks;

public class DeckBuilderModelProvider : IDisposable
{
	private DeckBuilderModel _model;

	public DeckBuilderModel Model => _model ?? (_model = InitializeModel());

	private ICardDatabaseAdapter CardDatabase => Pantry.Get<ICardDatabaseAdapter>();

	private FormatManager FormatManager => Pantry.Get<FormatManager>();

	private InventoryManager InventoryManager => Pantry.Get<InventoryManager>();

	private CardViewBuilder CardViewBuilder => Pantry.Get<CardViewBuilder>();

	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	private DeckBuilderCardFilterProvider FilterProvider => Pantry.Get<DeckBuilderCardFilterProvider>();

	private IPreferredPrintingDataProvider PreferredPrintingDataProvider => Pantry.Get<IPreferredPrintingDataProvider>();

	private DecksManager DeckManager => Pantry.Get<DecksManager>();

	private CosmeticsProvider CosmeticsProvider => Pantry.Get<CosmeticsProvider>();

	public event Action<string> OnModelNameSet;

	public event Action<string, ArtCrop> OnDeckBoxTextureChanged;

	public event Action<string> OnDeckCardBackSet;

	public event Action<DeckBuilderPile, CardData> TitleInPileChangedQuantity;

	public event Action<DeckBuilderPile, CardData> CardModifiedInPile;

	public event Action OnDeckFormatSetResetPool;

	public static DeckBuilderModelProvider Create()
	{
		return new DeckBuilderModelProvider();
	}

	private DeckBuilderModelProvider()
	{
		ContextProvider.OnContextSet += ResetModel;
	}

	private DeckBuilderModel InitializeModel()
	{
		DeckBuilderContext context = ContextProvider.Context;
		Dictionary<uint, uint> cardPoolFromContext = CardPoolToDeckBuilderModelUtilities.GetCardPoolFromContext(context, FormatManager, InventoryManager, CardDatabase);
		Dictionary<uint, string> dictionary = context?.CardSkinOverride ?? new Dictionary<uint, string>();
		if (context != null && context.IsLimited && context.Event.PlayerEvent.CourseData.CardStyles != null)
		{
			dictionary = CardPoolToDeckBuilderModelUtilities.GetCardSkinOverridesFromEventData(CardDatabase, dictionary, context.Event?.PlayerEvent?.CourseData?.CardStyles ?? new List<string>(), cardPoolFromContext);
		}
		return new DeckBuilderModel(CardDatabase, context?.Deck, cardPoolFromContext, context?.IsConstructed ?? false, context?.IsSideboarding ?? false, (uint)(context?.MaxCardsByTitle ?? Pantry.Get<FormatManager>().GetDefaultFormat().MaxCardsByTitle), (context != null && context.IsReadOnly) || (context?.OnlyShowPoolCards ?? false), dictionary, context?.Format?.FormatName);
	}

	public void ResetModel(DeckBuilderContext _)
	{
		ResetModel();
	}

	public void ResetModel()
	{
		_model = null;
	}

	private void OnAnyModelChange()
	{
		WrapperDeckBuilder.CacheDeck(Model, ContextProvider.Context);
	}

	public void SetDeckName(string name)
	{
		if (ContextProvider.Context != null)
		{
			Model._deckName = name;
			this.OnModelNameSet?.Invoke(name);
			OnAnyModelChange();
		}
	}

	public void SetDeckBoxTextureByCardData(MetaCardView view)
	{
		SetDeckBoxTextureByCardData(view.Card);
	}

	public void SetDeckBoxTextureByCardData(CardData data)
	{
		if (ContextProvider.Context != null && !ContextProvider.Context.IsReadOnly)
		{
			Model._deckTileId = data.GrpId;
			Model._deckArtId = data.Printing.ArtId;
			var (arg, arg2) = GetDeckBoxTextureInformation(CardDatabase, CardViewBuilder, Model._deckTileId, Model._deckArtId);
			OnAnyModelChange();
			this.OnDeckBoxTextureChanged?.Invoke(arg, arg2);
		}
	}

	public static (string, ArtCrop) GetDeckBoxTextureInformation(ICardDatabaseAdapter cardDatabase, CardViewBuilder cardViewBuilder, uint grpId, uint artId)
	{
		CardPrintingData cardPrintingData = null;
		if (artId != 0)
		{
			cardPrintingData = cardDatabase.DatabaseUtilities.GetPrintingsByArtId(artId)?.FirstOrDefault();
		}
		else if (grpId != 0)
		{
			cardPrintingData = cardDatabase.CardDataProvider.GetCardPrintingById(grpId);
		}
		string text = cardPrintingData?.ImageAssetPath ?? string.Empty;
		ArtCrop item = cardViewBuilder.CardMaterialBuilder.CropDatabase.GetCrop(text, "Normal") ?? ArtCrop.DEFAULT;
		return (text, item);
	}

	public void SetSelectedSleeve(string cardBack)
	{
		if (ContextProvider.Context != null)
		{
			string text = cardBack;
			if (text == DeckManager?.GetDefaultSleeve())
			{
				text = null;
			}
			Model._cardBack = text;
			OnAnyModelChange();
			this.OnDeckCardBackSet?.Invoke(text);
		}
	}

	public void SetSelectedAvatar(AvatarSelection avatar)
	{
		if (ContextProvider.Context != null)
		{
			string text = avatar.Id;
			if (text == CosmeticsProvider.PlayerAvatarSelection)
			{
				text = null;
			}
			Model._avatar = text;
			OnAnyModelChange();
		}
	}

	public void SetSelectedPet(PetEntry petEntry)
	{
		if (ContextProvider.Context != null)
		{
			ClientPetSelection playerPetSelection = CosmeticsProvider.PlayerPetSelection;
			PetEntry petEntry2 = petEntry;
			if (petEntry != null && playerPetSelection != null && petEntry.Name == playerPetSelection.name && petEntry.Variant == playerPetSelection.variant)
			{
				petEntry2 = null;
			}
			string pet = ((petEntry2 == null) ? null : (petEntry.Name + "." + petEntry.Variant));
			Model._pet = pet;
			OnAnyModelChange();
		}
	}

	public void SetSelectedEmotes(List<string> emoteList)
	{
		if (ContextProvider.Context != null)
		{
			Model._emotes = new List<string>();
			if (emoteList != null)
			{
				Model._emotes.AddRange(emoteList);
			}
			OnAnyModelChange();
		}
	}

	public void OnDefaultCosmeticSelected(CosmeticType cosmeticType)
	{
		switch (cosmeticType)
		{
		case CosmeticType.Avatar:
			Model._avatar = null;
			break;
		case CosmeticType.Pet:
			Model._pet = null;
			break;
		case CosmeticType.Sleeve:
			Model._cardBack = null;
			this.OnDeckCardBackSet?.Invoke(Model._cardBack);
			break;
		case CosmeticType.Emote:
			Model._emotes = new List<string>();
			break;
		}
		OnAnyModelChange();
	}

	public bool CanAddCardToMainDeck(uint grpId)
	{
		return Model.CanAddCardToMainDeck(grpId);
	}

	public void AddCardToDeckPile(DeckBuilderPile pile, CardData card, ICardRolloverZoom zoomHandler, bool fromSpecializePopup = false)
	{
		if (Model == null)
		{
			return;
		}
		if (!fromSpecializePopup)
		{
			CardPrintingData item = SpecializeUtilities.GetBasePrinting(CardDatabase.CardDataProvider, card.GrpId).BasePrinting;
			if (item.GrpId != card.GrpId)
			{
				card = CardDataExtensions.CreateSkinCard(item.GrpId, CardDatabase, card.SkinCode);
			}
		}
		CardPrintingData cardPrintingById = CardDatabase.CardDataProvider.GetCardPrintingById(card.GrpId);
		bool flag = Model.CanAddCardToPile(pile, card.GrpId);
		if (!flag)
		{
			return;
		}
		if (CompanionUtil.InvalidInFormat(CardDatabase, card.Printing, ContextProvider.Context.Format, out var errorText) && pile == DeckBuilderPile.Companion)
		{
			SystemMessageManager.Instance.ShowOk((MTGALocalizedString)"MainNav/DeckBuilder/CompanionInvalid_Title", errorText);
			return;
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_add_card, AudioManager.Default);
		zoomHandler.Close();
		if (flag)
		{
			Model.AddCardToPile(pile, card);
			Model.UpdatePile(pile);
			this.TitleInPileChangedQuantity?.Invoke(pile, card);
		}
		SetCardSkin(cardPrintingById, card.SkinCode);
		switch (pile)
		{
		case DeckBuilderPile.Commander:
		case DeckBuilderPile.Partner:
		{
			bool value = DeckBuilderWidgetUtilities.HasCommanderSet(ContextProvider.Context, _model) == DeckBuilderWidgetUtilities.CommanderType.PartnerableCommander;
			FilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Commanders, value);
			FilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
			break;
		}
		case DeckBuilderPile.Companion:
			FilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Companions, value: false);
			FilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
			break;
		}
		VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
		VisualsUpdater.UpdateAllDeckVisuals();
		if ((Model._deckTileId == 0 && !CardUtilities.IsBasicLand(card)) || pile == DeckBuilderPile.Commander)
		{
			SetDeckBoxTextureByCardData(card);
		}
		this.CardModifiedInPile?.Invoke(pile, card);
		if (pile != DeckBuilderPile.Commander || !SpecializeUtilities.IsSpecializeBaseCard(card.Printing))
		{
			OnAnyModelChange();
		}
	}

	public void RemoveCardFromPile(DeckBuilderPile pile, ICardRolloverZoom zoomHandler, CardData card, int count = 1)
	{
		if (Model != null)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_remove_card, AudioManager.Default);
			zoomHandler.Close();
			for (int i = 0; i < count; i++)
			{
				Model.RemoveCardFromPile(ContextProvider.Context, pile, card.GrpId);
			}
			Model.UpdatePile(pile);
			if (pile == DeckBuilderPile.Commander || pile == DeckBuilderPile.Companion || pile == DeckBuilderPile.Partner)
			{
				FilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
			}
			this.TitleInPileChangedQuantity?.Invoke(pile, card);
			VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
			VisualsUpdater.UpdateAllDeckVisuals();
			OnAnyModelChange();
		}
	}

	public void MoveCardFromPileToPile(DeckBuilderPile fromPile, DeckBuilderPile toPile, ICardRolloverZoom zoomHandler, CardData card, int count = 1, bool fromSpecializePopup = false)
	{
		if (Model == null)
		{
			return;
		}
		if (!fromSpecializePopup)
		{
			CardPrintingData item = SpecializeUtilities.GetBasePrinting(CardDatabase.CardDataProvider, card.GrpId).BasePrinting;
			if (item.GrpId != card.GrpId)
			{
				card = CardDataExtensions.CreateSkinCard(item.GrpId, CardDatabase, card.SkinCode);
			}
		}
		CardPrintingData cardPrintingById = CardDatabase.CardDataProvider.GetCardPrintingById(card.GrpId);
		if (CompanionUtil.InvalidInFormat(CardDatabase, card.Printing, ContextProvider.Context.Format, out var errorText) && toPile == DeckBuilderPile.Companion)
		{
			SystemMessageManager.Instance.ShowOk((MTGALocalizedString)"MainNav/DeckBuilder/CompanionInvalid_Title", errorText);
			return;
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_remove_card, AudioManager.Default);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_add_card, AudioManager.Default);
		zoomHandler.Close();
		for (int i = 0; i < count; i++)
		{
			Model.RemoveCardFromPile(ContextProvider.Context, fromPile, card.GrpId);
		}
		if (fromPile == DeckBuilderPile.Commander || fromPile == DeckBuilderPile.Companion)
		{
			FilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
		}
		this.TitleInPileChangedQuantity?.Invoke(fromPile, card);
		bool flag = Model.CanAddCardToPile(toPile, card.GrpId);
		if (!flag)
		{
			Model.AddCardToPile(fromPile, card);
			return;
		}
		Model.UpdatePile(fromPile);
		if (flag)
		{
			Model.AddCardToPile(toPile, card);
			Model.UpdatePile(toPile);
			this.TitleInPileChangedQuantity?.Invoke(toPile, card);
		}
		SetCardSkin(cardPrintingById, card.SkinCode);
		switch (toPile)
		{
		case DeckBuilderPile.Commander:
			FilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Commanders, value: false);
			FilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
			break;
		case DeckBuilderPile.Companion:
			FilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Companions, value: false);
			FilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
			break;
		}
		if ((Model._deckTileId == 0 && !CardUtilities.IsBasicLand(card)) || toPile == DeckBuilderPile.Commander)
		{
			SetDeckBoxTextureByCardData(card);
		}
		this.CardModifiedInPile?.Invoke(toPile, card);
		if (toPile != DeckBuilderPile.Commander || !SpecializeUtilities.IsSpecializeBaseCard(card.Printing))
		{
			VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
			VisualsUpdater.UpdateAllDeckVisuals();
			OnAnyModelChange();
		}
	}

	public void SetCardSkin(CardPrintingData printing, string skinCode)
	{
		if (skinCode != null && !DeckBuilderWidgetUtilities.OwnsOrHasSkinInPool(CosmeticsProvider, _model, CardDatabase, printing.ArtId, skinCode))
		{
			skinCode = null;
		}
		Model.SetCardSkin(printing.GrpId, skinCode);
	}

	public void AssignUnassignedCardSkinsToDeck()
	{
		IPreferredPrintingDataProvider preferredPrintingDataProvider = Pantry.Get<IPreferredPrintingDataProvider>();
		List<CardPrintingQuantity> allFilteredCards = Model.GetAllFilteredCards();
		bool isLimited = ContextProvider.Context.IsLimited;
		foreach (IGrouping<uint, CardPrintingQuantity> item in from i in allFilteredCards
			group i by i.Printing.TitleId)
		{
			CardPrintingQuantity cardPrintingQuantity = item.OrderByDescending((CardPrintingQuantity i) => i.Printing.GrpId).First();
			(CardPrintingData, string) replacementStyle = GetReplacementStyle(CardDatabase, CosmeticsProvider, cardPrintingQuantity.Printing);
			PreferredPrintingWithStyle preferredPrintingForTitleId = preferredPrintingDataProvider.GetPreferredPrintingForTitleId((int)item.Key);
			foreach (CardPrintingQuantity item2 in item)
			{
				if (Model.GetCardSkin(item2.Printing.GrpId) == null)
				{
					if (preferredPrintingForTitleId != null)
					{
						CardPrintingData cardPrintingById = CardDatabase.CardDataProvider.GetCardPrintingById((uint)preferredPrintingForTitleId.printingGrpId);
						SetCardAndStyleInDeck(item2, cardPrintingById, preferredPrintingForTitleId.styleCode);
					}
					else if (replacementStyle.Item2 != null && !isLimited)
					{
						SetCardAndStyleInDeck(item2, replacementStyle.Item1, replacementStyle.Item2);
					}
				}
			}
		}
	}

	public void ReplaceCardPoolWithFavorites()
	{
		if (!ContextProvider.Context.IsLimited)
		{
			return;
		}
		IPreferredPrintingDataProvider preferredPrintingDataProvider = Pantry.Get<IPreferredPrintingDataProvider>();
		foreach (IGrouping<uint, CardPrintingQuantity> item in from i in Model.GetFilteredPool().ToList()
			group i by i.Printing.TitleId)
		{
			PreferredPrintingWithStyle preferredPrintingForTitleId = preferredPrintingDataProvider.GetPreferredPrintingForTitleId((int)item.Key);
			foreach (CardPrintingQuantity item2 in item)
			{
				if (preferredPrintingForTitleId != null)
				{
					CardPrintingData cardPrintingById = CardDatabase.CardDataProvider.GetCardPrintingById((uint)preferredPrintingForTitleId.printingGrpId);
					SetCardAndStyleInPool(item2, cardPrintingById, preferredPrintingForTitleId.styleCode);
				}
			}
		}
	}

	public void ReplaceCardsByTitleId(CardPrintingData printing, string skinCode = null)
	{
		foreach (CardPrintingQuantity item in (from c in Model.GetAllFilteredCards()
			where c.Printing.TitleId == printing.TitleId
			select c).ToList())
		{
			SetCardAndStyleInDeck(item, printing, skinCode);
		}
	}

	private void SetCardAndStyleInDeck(CardPrintingQuantity item, CardPrintingData replacement, string replacementStyleCode = null)
	{
		if (item.Printing.GrpId != replacement.GrpId)
		{
			uint quantityInMainDeck = Model.GetQuantityInMainDeck(item.Printing.GrpId);
			if (quantityInMainDeck != 0)
			{
				Model.RemoveCardFromMainDeck(item.Printing.GrpId, quantityInMainDeck);
				Model.AddCardToMainDeck(replacement.GrpId, quantityInMainDeck);
				Model.UpdateMainDeck();
			}
			uint quantityInSideboard = Model.GetQuantityInSideboard(item.Printing.GrpId);
			if (quantityInSideboard != 0)
			{
				Model.RemoveCardFromSideboard(item.Printing.GrpId, quantityInSideboard);
				Model.AddCardToSideboard(replacement.GrpId, quantityInSideboard);
				Model.UpdateSideboard();
			}
			if (Model.GetFilteredCommandZone().Exists((CardPrintingQuantity i) => i.Printing.GrpId == item.Printing.GrpId))
			{
				Model.RemoveCardFromCommandZone(item.Printing.GrpId);
				Model.AddCardToCommandZone(replacement.GrpId);
				Model.UpdateCommandZone();
			}
		}
		SetCardSkinForLoad(replacement, replacementStyleCode);
	}

	private void SetCardAndStyleInPool(CardPrintingQuantity item, CardPrintingData replacement, string replacementStyleCode = null)
	{
		if (item.Printing.GrpId != replacement.GrpId)
		{
			uint quantityInCardPool = Model.GetQuantityInCardPool(item.Printing.GrpId);
			if (quantityInCardPool != 0)
			{
				Model.RemoveCardFromPool(item.Printing.GrpId, quantityInCardPool);
				Model.AddCardToPool(replacement.GrpId, quantityInCardPool);
				Model.UpdatePool();
			}
			SetCardSkinForLoad(replacement, replacementStyleCode);
		}
	}

	private void SetCardSkinForLoad(CardPrintingData cardPrintingData, string styleCode)
	{
		uint? num = CardDatabase.CardDataProvider.GetCardPrintingById(cardPrintingData.GrpId)?.ArtId;
		if (styleCode != null && num.HasValue && DeckBuilderWidgetUtilities.OwnsOrHasSkinInPool(CosmeticsProvider, Model, CardDatabase, num.Value, styleCode))
		{
			Model.SetCardSkin(cardPrintingData.GrpId, styleCode);
		}
	}

	public static (CardPrintingData printing, string highestSkin) GetReplacementStyle(ICardDatabaseAdapter cardDb, CosmeticsProvider cosmeticsProvider, CardPrintingData card)
	{
		foreach (CardPrintingData item in from p in cardDb.DatabaseUtilities.GetPrintingsByTitleId(card.TitleId)
			where card.IsRebalanced == p.IsRebalanced && p.IsPrimaryCard && CardUtilities.CanCardExistInDeck(p)
			orderby p.GrpId descending
			select p)
		{
			string highestTierCollectedArtStyle = cosmeticsProvider.GetHighestTierCollectedArtStyle(item.ArtId);
			if (highestTierCollectedArtStyle != null)
			{
				return (printing: item, highestSkin: highestTierCollectedArtStyle);
			}
		}
		return (printing: null, highestSkin: null);
	}

	public void SetDeckFormat(DeckFormat format)
	{
		DeckBuilderContext context = ContextProvider.Context;
		CompanionUtil companionUtil = Pantry.Get<CompanionUtil>();
		DeckBuilderLayoutState deckBuilderLayoutState = Pantry.Get<DeckBuilderLayoutState>();
		DeckBuilderPreferredPrintingState deckBuilderPreferredPrintingState = Pantry.Get<DeckBuilderPreferredPrintingState>();
		Model.UpdateFormatInfo(format.FormatName, (uint)format.MaxCardsByTitle);
		Model.SwapRebalancedCards(context);
		companionUtil.UpdateValidation(Model, context.Format);
		deckBuilderLayoutState.UpdateSideboardVisibility();
		if (context.IsEditingDeck)
		{
			if (format.FormatIncludesCommandZone && Model.GetFilteredCommandZone().Count == 0)
			{
				FilterProvider.SetFilter(FilterValueChangeSource.Miscellaneous, CardFilterType.Commanders, value: true);
			}
			else if (!format.FormatIncludesCommandZone && Model.GetFilteredCommandZone().Count > 0)
			{
				Model.ClearCommandZone();
			}
			VisualsUpdater.UpdateAllDeckVisuals();
		}
		this.OnDeckFormatSetResetPool?.Invoke();
		deckBuilderPreferredPrintingState.CollapseAllExpandedPoolCards();
		if (context.IsEditingDeck && context.Format.FormatIncludesCommandZone && Model.GetFilteredCommandZone().Count == 0)
		{
			FilterProvider.ResetAndApplyFilters(FilterValueChangeSource.Miscellaneous);
		}
		else
		{
			FilterProvider.ApplyFilters(FilterValueChangeSource.Miscellaneous);
		}
		OnAnyModelChange();
	}

	public void Dispose()
	{
		ContextProvider.OnContextSet -= ResetModel;
	}
}
