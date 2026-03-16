using System;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Shared.Code.CardFilters;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;

internal interface IDraftMetaCardHolder
{
	ICardRolloverZoom RolloverZoomView { get; set; }

	Func<MetaCardView, bool> CanSingleClickCards { get; set; }

	Func<MetaCardView, bool> CanDoubleClickCards { get; set; }

	Func<MetaCardView, bool> CanDragCards { get; set; }

	Func<MetaCardView, bool> CanDropCards { get; set; }

	Func<MetaCardView, bool> ShowHighlight { get; set; }

	Action<MetaCardView> OnCardClicked { get; set; }

	Action<MetaCardView> OnCardRightClicked { get; set; }

	Action<MetaCardView> OnCardDragged { get; set; }

	Action<MetaCardView, MetaCardHolder> OnCardDropped { get; set; }

	Action<MetaCardView, bool, bool, bool> CustomHighlightHandler { get; set; }

	void EnsureInit(ICardRolloverZoom cardZoom, AssetLookupSystem assetLookupSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder);

	void SetCards(List<ListMetaCardViewDisplayInformation> newDisplayInfo, CardFilter textCardFilter = null);

	List<MetaCardHolder> GetMetaCardHolderList();

	void ResetLanguage();
}
