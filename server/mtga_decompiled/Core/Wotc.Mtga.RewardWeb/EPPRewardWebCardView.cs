using System;
using System.Collections.Generic;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.RewardWeb;

public class EPPRewardWebCardView : MonoBehaviour
{
	[Serializable]
	public class CardViewLayout
	{
		public int numberofCards;

		public List<Transform> anchors;

		public void Hide()
		{
			foreach (Transform anchor in anchors)
			{
				anchor.gameObject.SetActive(value: false);
			}
		}
	}

	private CardDatabase _cardDatabase;

	private CardViewBuilder _cardViewBuilder;

	[SerializeField]
	private RewardWebMetaCardHolder _cardHolder;

	private ICardRolloverZoom _zoomHandler;

	[SerializeField]
	private List<CardViewLayout> _layouts;

	private CardViewLayout _currentLayout;

	private Dictionary<Transform, CDCMetaCardView> _cardViewsMap = new Dictionary<Transform, CDCMetaCardView>();

	private bool _initialize;

	public void Initialize(CardViewBuilder cardViewBuilder, ICardRolloverZoom zoomHandler, CardDatabase cardDatabase)
	{
		if (!_initialize)
		{
			_initialize = true;
			_cardHolder.EnsureInit(cardDatabase, cardViewBuilder);
			_cardHolder.RolloverZoomView = _zoomHandler;
			if (zoomHandler != null)
			{
				SetZoomHandler(zoomHandler);
			}
			_cardHolder.ShowHighlight = (MetaCardView cardView) => false;
			_cardViewBuilder = cardViewBuilder;
			_cardDatabase = cardDatabase;
			hideAllLayouts();
		}
	}

	private void hideAllLayouts()
	{
		foreach (CardViewLayout layout in _layouts)
		{
			layout.Hide();
		}
	}

	public void SetZoomHandler(ICardRolloverZoom zoomHandler)
	{
		_zoomHandler = zoomHandler;
		_cardHolder.RolloverZoomView = _zoomHandler;
	}

	public void SetCards(List<uint> cards, string styleName = null)
	{
		HideLayout(_currentLayout);
		_currentLayout = LayoutForCards(cards.Count);
		SetupCards(_currentLayout);
		_cardHolder.ClearCards();
		CardCollection cardCollection = new CardCollection(_cardHolder.CardDatabase);
		for (int i = 0; i < _currentLayout.anchors.Count; i++)
		{
			Transform transform = _currentLayout.anchors[i];
			CDCMetaCardView cardview = CardViewForAnchor(transform);
			CardPrintingData cardPrintingById = _cardDatabase.CardDataProvider.GetCardPrintingById(cards[i]);
			transform.gameObject.SetActive(value: true);
			ShowCardInHanger(cardCollection, cardview, cardPrintingById, styleName);
		}
		_cardHolder.SetCards(cardCollection);
	}

	private CDCMetaCardView CardViewForAnchor(Transform anchor)
	{
		_cardViewsMap.TryGetValue(anchor, out var value);
		return value;
	}

	private void HideLayout(CardViewLayout currentLayout)
	{
		currentLayout?.Hide();
	}

	private CardViewLayout LayoutForCards(int cardsCount)
	{
		foreach (CardViewLayout layout in _layouts)
		{
			if (layout.numberofCards == cardsCount)
			{
				return layout;
			}
		}
		return _layouts[_layouts.Count - 1];
	}

	private void SetupCards(CardViewLayout layout)
	{
		foreach (Transform anchor in layout.anchors)
		{
			if (!_cardViewsMap.TryGetValue(anchor, out var value))
			{
				value = _cardViewBuilder.CreateCDCMetaCardView(null, anchor);
				value.Init(_cardDatabase, _cardViewBuilder);
				value.gameObject.SetActive(value: false);
				value.Holder = _cardHolder;
				_cardViewsMap[anchor] = value;
			}
			else
			{
				value.gameObject.SetActive(value: false);
			}
			anchor.gameObject.SetActive(value: false);
		}
	}

	private void ShowCardInHanger(CardCollection collection, CDCMetaCardView cardview, CardPrintingData printingData, string cardStyle)
	{
		CardData cardData = CardDataExtensions.CreateSkinCard(printingData.GrpId, _cardDatabase, cardStyle);
		cardData.IsFakeStyleCard = !string.IsNullOrEmpty(cardStyle);
		cardview.gameObject.SetActive(value: false);
		cardview.SetData(cardData);
		cardview.gameObject.SetActive(value: true);
		cardview.Holder.CanDragCards = (MetaCardView cardView) => false;
		collection.Add(cardData, 1);
	}

	public void DisableCards()
	{
		HideLayout(_currentLayout);
	}
}
