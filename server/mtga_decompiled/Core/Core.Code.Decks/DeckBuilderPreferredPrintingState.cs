using System;
using System.Collections.Generic;
using GreClient.CardData;
using Wizards.Arena.Promises;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.PreferredPrinting;
using Wotc.Mtga.Cards.Database;

namespace Core.Code.Decks;

public class DeckBuilderPreferredPrintingState
{
	public bool ExpandAllOnce;

	private DeckBuilderModelProvider ModelProvider => Pantry.Get<DeckBuilderModelProvider>();

	private DeckBuilderVisualsUpdater VisualsUpdater => Pantry.Get<DeckBuilderVisualsUpdater>();

	public HashSet<uint> ExpandedPoolCards { get; } = new HashSet<uint>();

	public event Action OnPreferredPrintingsChanged;

	public static DeckBuilderPreferredPrintingState Create()
	{
		return new DeckBuilderPreferredPrintingState();
	}

	public bool IsExpanded(uint titleId)
	{
		if (!ExpandAllOnce)
		{
			return ExpandedPoolCards.Contains(titleId);
		}
		return true;
	}

	public void ExpandCard(PagesMetaCardView cardView)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_preferred_printing_expand, cardView.gameObject);
		ExpandedPoolCards.Add(cardView.TitleId);
		VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
	}

	public void CollapseCard(PagesMetaCardView cardView)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_preferred_printing_retract, cardView.gameObject);
		ExpandedPoolCards.Remove(cardView.TitleId);
		uint? collapseTitleId = null;
		if (cardView.GetLastDisplayInfoStyle() == PagesMetaCardView.ExpandedDisplayStyle.Expanded_Last)
		{
			collapseTitleId = cardView.TitleId;
		}
		VisualsUpdater.RefreshPoolView(scrollToTop: false, collapseTitleId);
	}

	public void CollapseAllExpandedPoolCards()
	{
		ExpandedPoolCards.Clear();
		VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
	}

	public void OnCardPreferredPrintingToggleClicked(PagesMetaCardView cardView, bool toggleValue)
	{
		ICardDatabaseAdapter cardDatabaseAdapter = Pantry.Get<ICardDatabaseAdapter>();
		IPreferredPrintingDataProvider preferredPrintingDataProvider = Pantry.Get<IPreferredPrintingDataProvider>();
		DeckBuilderModel model = ModelProvider.Model;
		uint titleId = cardView.TitleId;
		uint grpId = cardView.GetGrpId();
		string skinCode = cardView.Card.SkinCode;
		PreferredPrintingWithStyle preferredPrintingForTitleId = preferredPrintingDataProvider.GetPreferredPrintingForTitleId((int)titleId);
		bool isPreferredPrinting = cardView.GetIsPreferredPrinting(titleId, grpId, skinCode);
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_preferred_printing_select_card, cardView.gameObject);
		if (toggleValue && !isPreferredPrinting)
		{
			if (preferredPrintingForTitleId != null)
			{
				preferredPrintingDataProvider.RemovePreferredPrintingForTitleId((int)titleId);
			}
			Promise<bool> promise = preferredPrintingDataProvider.SetPreferredPrintingForTitleId((int)titleId, (int)grpId, skinCode);
			IReadOnlyList<CardPrintingData> readOnlyList2;
			if (!AltPrintingUtilities.FindBasePrinting(cardView.Card.Printing, cardDatabaseAdapter.CardDataProvider, cardDatabaseAdapter.AltPrintingProvider, out var basePrinting, Pantry.Get<StoreManager>().CardSkinCatalog))
			{
				IReadOnlyList<CardPrintingData> readOnlyList = new CardPrintingData[1] { cardView.Card.Printing };
				readOnlyList2 = readOnlyList;
			}
			else
			{
				readOnlyList2 = cardDatabaseAdapter.DatabaseUtilities.GetPrintingsByArtId(basePrinting.ArtId);
			}
			foreach (CardPrintingData item in readOnlyList2)
			{
				model.SetCardSkin(item.GrpId, skinCode);
			}
			if (model.HasLoadedDeck)
			{
				VisualsUpdater.UpdateAllDeckVisuals();
			}
			VisualsUpdater.RefreshPoolView(scrollToTop: false, null);
			if (!promise.Error.IsError)
			{
				BI_PreferredPrintingSet(titleId.ToString(), grpId.ToString(), preferredPrintingForTitleId?.printingGrpId.ToString(), isPreferring: true);
			}
		}
		else if (!toggleValue && isPreferredPrinting && !preferredPrintingDataProvider.RemovePreferredPrintingForTitleId((int)titleId).Error.IsError && preferredPrintingForTitleId != null)
		{
			BI_PreferredPrintingSet(titleId.ToString(), null, preferredPrintingForTitleId.printingGrpId.ToString(), isPreferring: false);
		}
		this.OnPreferredPrintingsChanged?.Invoke();
	}

	private void BI_PreferredPrintingSet(string titleID, string newGrpID, string oldGrpID, bool isPreferring)
	{
		PreferredPrintingSet payload = new PreferredPrintingSet
		{
			EventTime = DateTime.Now,
			TitleID = titleID,
			NewGrpID = newGrpID,
			OldGrpID = oldGrpID,
			IsPreferring = isPreferring
		};
		Pantry.Get<IBILogger>().Send(ClientBusinessEventType.PreferredPrintingSet, payload);
	}
}
