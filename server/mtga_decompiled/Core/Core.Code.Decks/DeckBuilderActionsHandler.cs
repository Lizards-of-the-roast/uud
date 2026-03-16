using System;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Cosmetics;
using Core.Shared.Code.CardFilters;
using GreClient.CardData;
using UnityEngine.EventSystems;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Client.Models.Catalog;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Providers;

namespace Core.Code.Decks;

public class DeckBuilderActionsHandler
{
	private DeckBuilderContextProvider ContextProvider => Pantry.Get<DeckBuilderContextProvider>();

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderLayoutState LayoutState => Pantry.Get<DeckBuilderLayoutState>();

	private DeckBuilderCardFilterProvider FilterProvider => Pantry.Get<DeckBuilderCardFilterProvider>();

	private CardDatabase CardDatabase => Pantry.Get<CardDatabase>();

	public event Action<StaticColumnMetaCardView, bool> ShowQuantityAdjust;

	public event Action<bool> OnHideQuantityAdjust;

	public event Action OnCardRightClicked;

	public event Action<CardPrintingData, string, int, Action<string>> CardViewerRequested;

	public event Action DeckDetailsRequested;

	public event Action<CardDatabase, DisplayCosmeticsTypes> DeckDetailsCosmeticsSelectorRequested;

	public static DeckBuilderActionsHandler Create()
	{
		return new DeckBuilderActionsHandler();
	}

	public void HideQuantityAdjust()
	{
		this.OnHideQuantityAdjust?.Invoke(obj: true);
	}

	public void OnCardDropped(MetaCardView cardView, MetaCardHolder destinationHolder, ICardRolloverZoom zoomHandler)
	{
		DeckBuilderContext context = ContextProvider.Context;
		if (context.IsReadOnly)
		{
			return;
		}
		DeckBuilderPile? parentDeckBuilderPile = cardView.Holder.GetParentDeckBuilderPile();
		DeckBuilderPile? parentDeckBuilderPile2 = destinationHolder.GetParentDeckBuilderPile();
		if (FilterProvider.Filter.IsSet(CardFilterType.Commanders) && DeckBuilderWidgetUtilities.HasCommanderSet(context, ModelProvider.Model) == DeckBuilderWidgetUtilities.CommanderType.PartnerableCommander && parentDeckBuilderPile2 != DeckBuilderPile.Partner && parentDeckBuilderPile2 != DeckBuilderPile.Commander)
		{
			return;
		}
		bool flag = parentDeckBuilderPile == DeckBuilderPile.MainDeck;
		if (parentDeckBuilderPile2 == DeckBuilderPile.MainDeck && flag)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_add_card, AudioManager.Default);
			zoomHandler.Close();
			VisualsUpdater.UpdateAllDeckVisuals();
			return;
		}
		bool flag2 = parentDeckBuilderPile2 == DeckBuilderPile.Pool;
		bool flag3 = parentDeckBuilderPile2 == DeckBuilderPile.Commander;
		bool flag4 = parentDeckBuilderPile2 == DeckBuilderPile.Partner;
		if (flag3 && !DeckFormat.CardCanBeCommander(cardView.Card.Printing))
		{
			ShowPopupErrorMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Invalid_Deck_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_HasInvalidCommander"), zoomHandler);
			return;
		}
		if (flag4 && !DeckFormat.CardCanBePartner(cardView.Card.Printing))
		{
			ShowPopupErrorMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Invalid_Deck_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_HasInvalidPartner"), zoomHandler);
			return;
		}
		if (flag3 && ModelProvider.Model.HasMultipleCommanders() && !DeckFormat.CardCanBePartner(cardView.Card.Printing))
		{
			ShowPopupErrorMessage(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Invalid_Deck_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/DeckBuilder/DeckBuilder_InvalidDeckReasons_CommanderHasPartner"), zoomHandler);
			return;
		}
		if (context.IsLimited && LayoutState.IsColumnViewExpanded && flag)
		{
			VisualsUpdater.UnsuggestedCardsInPool.Add(cardView.Card.Printing);
			VisualsUpdater.SuggestedCardsInDeck.Remove(cardView.Card.Printing);
		}
		if (parentDeckBuilderPile.HasValue && parentDeckBuilderPile2.HasValue)
		{
			int count = 1;
			if (flag && flag2 && LayoutState.LayoutInUse == DeckBuilderLayout.Column)
			{
				count = ((StaticColumnMetaCardView)cardView).Quantity;
			}
			else if (flag && flag2)
			{
				count = ((ListMetaCardView_Expanding)cardView).Quantity;
			}
			ModelProvider.MoveCardFromPileToPile(parentDeckBuilderPile.Value, parentDeckBuilderPile2.Value, zoomHandler, cardView.Card, count);
		}
	}

	public void OnCardClicked(MetaCardView cardView, ICardRolloverZoom zoomHandler)
	{
		DeckBuilderContext context = ContextProvider.Context;
		IReadOnlyCardFilter filter = FilterProvider.Filter;
		CardSkinCatalog cardSkinCatalog = Pantry.Get<StoreManager>().CardSkinCatalog;
		CosmeticsProvider cosmeticsProvider = Pantry.Get<CosmeticsProvider>();
		if (context.Mode == DeckBuilderMode.ReadOnlyCollection || cardView.Card == null)
		{
			return;
		}
		if (context.CanCraft && (context.Mode != DeckBuilderMode.DeckBuilding || !context.IsEditingDeck))
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_whoosh_01, cardView.gameObject);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_pull_card, cardView.gameObject);
			OpenCardViewer(cardView, zoomHandler);
		}
		else if (context.IsEditingDeck && context.Format.FormatIncludesCommandZone && filter.IsSet(CardFilterType.Commanders))
		{
			DeckBuilderPile pile = ((DeckBuilderWidgetUtilities.HasCommanderSet(context, ModelProvider.Model) == DeckBuilderWidgetUtilities.CommanderType.PartnerableCommander) ? DeckBuilderPile.Partner : DeckBuilderPile.Commander);
			ModelProvider.AddCardToDeckPile(pile, cardView.Card, zoomHandler);
		}
		else if (CompanionUtil.CardCanBeCompanion(cardView.Card.Printing) && ModelProvider.Model.GetCompanion() == null && ModelProvider.Model.GetQuantityInWholeDeck(cardView.Card.Printing.GrpId) == 0)
		{
			if (CompanionUtil.InvalidInFormat(CardDatabase, cardView.Card.Printing, context.Format, out var errorText))
			{
				SystemMessageManager.Instance.ShowOk((MTGALocalizedString)"MainNav/DeckBuilder/CompanionInvalid_Title", errorText, delegate
				{
					ModelProvider.AddCardToDeckPile(DeckBuilderPile.MainDeck, cardView.Card, zoomHandler);
				});
			}
			else if (filter.IsSet(CardFilterType.Companions))
			{
				ModelProvider.AddCardToDeckPile(DeckBuilderPile.Companion, cardView.Card, zoomHandler);
			}
			else
			{
				SystemMessageManager.Instance.ShowMessage((MTGALocalizedString)"MainNav/DeckBuilder/CompanionSelection_Title", new MTGALocalizedString
				{
					Key = "MainNav/DeckBuilder/CompanionSelection_Body",
					Parameters = new Dictionary<string, string> { 
					{
						"conditionText",
						CompanionUtil.GetAbilityText(cardView.Card.Printing, CardDatabase.GreLocProvider)
					} }
				}, (MTGALocalizedString)"DuelScene/ClientPrompt/ClientPrompt_Button_No", delegate
				{
					ModelProvider.AddCardToDeckPile(DeckBuilderPile.MainDeck, cardView.Card, zoomHandler);
				}, (MTGALocalizedString)"DuelScene/ClientPrompt/ClientPrompt_Button_Yes", delegate
				{
					ModelProvider.AddCardToDeckPile(DeckBuilderPile.Companion, cardView.Card, zoomHandler);
				});
			}
		}
		else
		{
			CardData card = cardView.Card;
			CardPrintingData basePrinting;
			uint artId = (AltPrintingUtilities.FindBasePrinting(card.Printing, CardDatabase.CardDataProvider, CardDatabase.AltPrintingProvider, out basePrinting, cardSkinCatalog) ? basePrinting.ArtId : card.Printing.ArtId);
			bool flag = !string.IsNullOrEmpty(card.SkinCode);
			bool flag2 = cosmeticsProvider.OwnsSkin(artId, card.SkinCode);
			if (!context.IsSideboarding && !context.IsLimited && flag && !flag2)
			{
				OpenCardViewer(cardView, zoomHandler);
				return;
			}
			CardData card2 = (flag ? CardDataExtensions.CreateSkinCard(basePrinting.GrpId, CardDatabase, card.SkinCode) : card);
			DeckBuilderPile pile2 = (LayoutState.IsListViewSideboarding ? DeckBuilderPile.Sideboard : DeckBuilderPile.MainDeck);
			ModelProvider.AddCardToDeckPile(pile2, card2, zoomHandler);
		}
		if (LayoutState.LayoutInUse == DeckBuilderLayout.Column)
		{
			this.OnHideQuantityAdjust?.Invoke(obj: false);
		}
	}

	public void CardRightClicked(MetaCardView cardView, ICardRolloverZoom zoomHandler)
	{
		if (ContextProvider.Context.CanCraft)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_whoosh_01, cardView.gameObject);
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_pull_card, cardView.gameObject);
			this.OnCardRightClicked?.Invoke();
			OpenCardViewer(cardView, zoomHandler);
		}
		else
		{
			zoomHandler.CardPointerDown(PointerEventData.InputButton.Right, cardView.Card, cardView);
		}
	}

	public void DeckSideboard_OnCardAddClicked(MetaCardView cardView, ICardRolloverZoom zoomHandler)
	{
		ModelProvider.AddCardToDeckPile(DeckBuilderPile.Sideboard, cardView.Card, zoomHandler);
		this.OnHideQuantityAdjust?.Invoke(obj: true);
	}

	public void DeckSideboard_OnCardRemoveClicked(MetaCardView cardView, ICardRolloverZoom zoomHandler)
	{
		if (ContextProvider.Context.IsReadOnly)
		{
			return;
		}
		this.OnHideQuantityAdjust?.Invoke(obj: true);
		if (ContextProvider.Context.CanCraft && ContextProvider.Context.Mode != DeckBuilderMode.DeckBuilding)
		{
			OpenCardViewer(cardView, zoomHandler);
			return;
		}
		ModelProvider.RemoveCardFromPile(DeckBuilderPile.Sideboard, zoomHandler, cardView.Card);
		if (LayoutState.IsColumnViewExpanded && PlatformUtils.IsHandheld())
		{
			ModelProvider.AddCardToDeckPile(DeckBuilderPile.MainDeck, cardView.Card, zoomHandler);
		}
	}

	public void DeckCompanion_OnCardClicked(MetaCardView cardView, ICardRolloverZoom zoomHandler)
	{
		if (ContextProvider.Context.Mode != DeckBuilderMode.ReadOnly)
		{
			this.OnHideQuantityAdjust?.Invoke(obj: true);
			if (ContextProvider.Context.CanCraft && ContextProvider.Context.Mode != DeckBuilderMode.DeckBuilding)
			{
				OpenCardViewer(cardView, zoomHandler);
			}
			else
			{
				ModelProvider.RemoveCardFromPile(DeckBuilderPile.Companion, zoomHandler, cardView.Card);
			}
		}
	}

	public void DeckCommander_OnCardClicked(MetaCardView cardView, ICardRolloverZoom zoomHandler, bool isPartner)
	{
		if (ContextProvider.Context.Mode != DeckBuilderMode.ReadOnly)
		{
			this.OnHideQuantityAdjust?.Invoke(obj: true);
			if (ContextProvider.Context.CanCraft && ContextProvider.Context.Mode != DeckBuilderMode.DeckBuilding)
			{
				OpenCardViewer(cardView, zoomHandler);
				return;
			}
			DeckBuilderPile pile = (isPartner ? DeckBuilderPile.Partner : DeckBuilderPile.Commander);
			ModelProvider.RemoveCardFromPile(pile, zoomHandler, cardView.Card);
		}
	}

	public void DeckCommander_OnCardAddClicked(MetaCardView cardView, ICardRolloverZoom zoomHandler)
	{
		int num = 0;
		if (ContextProvider.Context.CanCraft && cardView.ShowUnCollectedTreatment)
		{
			int quantityInCardPool = (int)ModelProvider.Model.GetQuantityInCardPool(cardView.Card.GrpId);
			num = 1 - quantityInCardPool;
		}
		if (num > 0)
		{
			OpenCardViewer(cardView, zoomHandler, num);
		}
	}

	public void OpenCardViewer(MetaCardView cardView, ICardRolloverZoom zoomHandler, int quantityToCraft = 1)
	{
		zoomHandler.Close();
		CardPrintingData cardPrintingData = cardView.Card.Printing;
		this.CardViewerRequested?.Invoke(cardPrintingData, cardView.Card.SkinCode, quantityToCraft, OnSkinSelected);
		void OnSkinSelected(string ccv)
		{
			BI_CosmeticPrintingSet(cardPrintingData.TitleId, cardPrintingData.GrpId, ccv, ModelProvider.Model.GetCardSkin(cardPrintingData.GrpId));
			ModelProvider.ReplaceCardsByTitleId(cardPrintingData, ccv);
			VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
			if (ContextProvider.Context.IsEditingDeck && ModelProvider.Model.GetQuantityInWholeDeck(cardPrintingData.GrpId) != 0)
			{
				VisualsUpdater.UpdateAllDeckVisuals();
			}
			WrapperDeckBuilder.CacheDeck(ModelProvider.Model, ContextProvider.Context);
		}
	}

	public void OpenDeckDetails()
	{
		this.DeckDetailsRequested?.Invoke();
	}

	public void OpenDeckDetailsCosmeticsSelector(CardDatabase cardDatabase, DisplayCosmeticsTypes cosmeticType)
	{
		this.DeckDetailsCosmeticsSelectorRequested?.Invoke(cardDatabase, cosmeticType);
	}

	private void ShowPopupErrorMessage(string title, string message, ICardRolloverZoom zoomHandler)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_deck_add_card, AudioManager.Default);
		zoomHandler.Close();
		SystemMessageManager.Instance.ShowOk(title, message);
	}

	private void BI_CosmeticPrintingSet(uint titleID, uint grpID, string newCardVariant, string oldCardVariant)
	{
		PreferredPrintingWithStyle preferredPrintingForTitleId = Pantry.Get<IPreferredPrintingDataProvider>().GetPreferredPrintingForTitleId((int)titleID);
		bool isGrpIDPreferred = preferredPrintingForTitleId != null && preferredPrintingForTitleId.printingGrpId == (int)grpID;
		CosmeticPrintingSet payload = new CosmeticPrintingSet
		{
			EventTime = DateTime.Now,
			TitleID = titleID.ToString(),
			GrpID = grpID.ToString(),
			NewCardVariant = newCardVariant,
			OldCardVariant = oldCardVariant,
			IsGrpIDPreferred = isGrpIDPreferred
		};
		Pantry.Get<IBILogger>().Send(ClientBusinessEventType.CosmeticPrintingSet, payload);
	}
}
