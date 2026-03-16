using System;
using System.Collections.Generic;
using AssetLookupTree;
using UnityEngine;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;

public class DraftColumnMetaCardHolder : StaticColumnManager, IDraftMetaCardHolder
{
	[SerializeField]
	private List<MetaCardHolder> _dropTargets;

	public Func<MetaCardView, bool> ShowHighlight { get; set; }

	public Action<MetaCardView> OnCardRightClicked { get; set; }

	public Action<MetaCardView> OnCardDragged { get; set; }

	public void EnsureInit(ICardRolloverZoom zoomBase, AssetLookupSystem assetLookupSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		base.RolloverZoomView = zoomBase;
		Init(assetLookupSystem, cardDatabase, cardViewBuilder);
	}

	public List<MetaCardHolder> GetMetaCardHolderList()
	{
		List<MetaCardHolder> list = new List<MetaCardHolder>();
		list.AddRange(_dropTargets);
		foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
		{
			list.Add(allColumn);
		}
		foreach (StaticColumnDropTarget allNewColumnDropTarget in _allNewColumnDropTargets)
		{
			list.Add(allNewColumnDropTarget);
		}
		return list;
	}

	public void ResetLanguage()
	{
		foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
		{
			foreach (StaticColumnMetaCardView cardView in allColumn.CardViews)
			{
				cardView.SetData(cardView.Card);
			}
		}
	}
}
