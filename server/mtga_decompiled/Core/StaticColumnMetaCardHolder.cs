using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using Core.Shared.Code.CardFilters;
using DG.Tweening;
using GreClient.CardData;
using UnityEngine;
using Wotc.Mtga.Cards;
using Wotc.Mtga.Extensions;

public class StaticColumnMetaCardHolder : MetaCardHolder
{
	[SerializeField]
	private Transform _cardParent;

	public float CardHeight = 5.6f;

	public StaticColumnPool CardPool;

	[NonSerialized]
	public bool IsExpanded;

	[NonSerialized]
	public Vector3 CardStackRotation;

	[NonSerialized]
	public Vector3 CardInstantiationOffset;

	[NonSerialized]
	public float RepositionMoveDuration;

	private readonly List<StaticColumnMetaCardView> _cardViews = new List<StaticColumnMetaCardView>();

	private uint _droppingGrpId;

	private Vector3 _droppingAtPosition;

	private int _firstLowerStackIndex;

	[SerializeField]
	private float _filterSpacing = 9f;

	public float ColumnNumber { get; set; }

	public float UpperColumnHeight { get; set; }

	public float TotalHeight { get; private set; }

	public List<StaticColumnMetaCardView> CardViews => _cardViews;

	public bool ContainsCard(uint grpId)
	{
		return _cardViews.Exists(grpId, (StaticColumnMetaCardView v, uint outerId) => v.Card.GrpId == outerId);
	}

	public void RegisterPosition(uint grpId, Vector3 position)
	{
		_droppingGrpId = grpId;
		_droppingAtPosition = position;
	}

	public override void ClearCards()
	{
		for (int num = _cardViews.Count - 1; num >= 0; num--)
		{
			ReleaseCardView(_cardViews[num]);
		}
		_cardViews.Clear();
	}

	public void SetCards(AssetLookupSystem assetLookupSystem, List<ListMetaCardViewDisplayInformation> list, CardFilter textSearchFilter, uint? grpQuantityAdjust)
	{
		List<StaticColumnMetaCardView> list2 = new List<StaticColumnMetaCardView>(_cardViews);
		_cardViews.Clear();
		_firstLowerStackIndex = 0;
		UpperColumnHeight = 0f;
		IEnumerable<ListMetaCardViewDisplayInformation> enumerable = CardSorter.Sort(list.Where((ListMetaCardViewDisplayInformation listMetaCardViewDisplayInformation) => listMetaCardViewDisplayInformation.Quantity != 0), base.CardDatabase, SortType.CMCWithXLast, SortType.ColorOrder, SortType.ManaCostDifficulty, SortType.Title);
		if (textSearchFilter != null)
		{
			List<ListMetaCardViewDisplayInformation> list3 = new List<ListMetaCardViewDisplayInformation>();
			List<ListMetaCardViewDisplayInformation> list4 = new List<ListMetaCardViewDisplayInformation>();
			foreach (ListMetaCardViewDisplayInformation item2 in enumerable)
			{
				bool flag = true;
				foreach (Func<CardFilterGroup, CardFilterGroup> filterFunction in textSearchFilter.GetFilterFunctions())
				{
					if (!filterFunction(new CardFilterGroup(new List<CardFilterGroup.FilteredCard>
					{
						new CardFilterGroup.FilteredCard(item2.Card)
					})).AnyPassed())
					{
						flag = false;
						break;
					}
				}
				item2.Deprioritized = !flag;
				if (flag)
				{
					CardData card = CardDataExtensions.CreateSkinCard(item2.Card.GrpId, base.CardDatabase, item2.SkinCode);
					UpperColumnHeight += Math.Abs(SpacingOffsetToUseForCard(assetLookupSystem, card, (int)item2.Quantity, grpQuantityAdjust));
					list3.Add(item2);
				}
				else
				{
					list4.Add(item2);
				}
			}
			_firstLowerStackIndex = list3.Count;
			enumerable = list3.Concat(list4);
		}
		foreach (ListMetaCardViewDisplayInformation cardInfo in enumerable)
		{
			StaticColumnMetaCardView staticColumnMetaCardView = list2.FirstOrDefault((StaticColumnMetaCardView v) => v.Card.GrpId == cardInfo.Card.GrpId && v.Card.Instance?.SkinCode == cardInfo.SkinCode);
			if (staticColumnMetaCardView == null)
			{
				ICardCollectionItem item = new CardCollectionItem(CardDataExtensions.CreateSkinCard(cardInfo.Card.GrpId, base.CardDatabase, cardInfo.SkinCode), (int)cardInfo.Quantity);
				staticColumnMetaCardView = CardPool.Acquire(item, this, _cardParent);
				staticColumnMetaCardView.RecentlyCreated = true;
				staticColumnMetaCardView.transform.position = CardInstantiationOffset;
			}
			else
			{
				list2.Remove(staticColumnMetaCardView);
				staticColumnMetaCardView.Quantity = (int)cardInfo.Quantity;
				staticColumnMetaCardView.FrontOfColumn = false;
			}
			staticColumnMetaCardView.SetErrors(cardInfo.Banned, cardInfo.Unowned, cardInfo.Invalid, cardInfo.Suggested);
			staticColumnMetaCardView.HangerSituation = new HangerSituation
			{
				ContextualHangers = cardInfo.ContextualHangers
			};
			staticColumnMetaCardView.UpdateVisuals();
			staticColumnMetaCardView.Holder = this;
			_cardViews.Add(staticColumnMetaCardView);
		}
		if (_cardViews.Count > 0)
		{
			List<StaticColumnMetaCardView> cardViews = _cardViews;
			cardViews[cardViews.Count - 1].FrontOfColumn = true;
		}
		foreach (StaticColumnMetaCardView item3 in list2)
		{
			ReleaseCardView(item3);
		}
	}

	private void ReleaseCardView(StaticColumnMetaCardView cardView)
	{
		if ((bool)cardView)
		{
			cardView.Cleanup();
			UnityEngine.Object.Destroy(cardView.gameObject);
		}
	}

	public void RepositionAllCards(AssetLookupSystem assetLookupSystem, uint? grpQuantityAdjust)
	{
		float num = 0f;
		TotalHeight = 0f;
		bool flag = false;
		float num2 = SpacingOffsetToUseForCard(assetLookupSystem, null, 1, null);
		DOTween.Kill(this, complete: true);
		for (int i = 0; i < _cardViews.Count; i++)
		{
			StaticColumnMetaCardView staticColumnMetaCardView = _cardViews[i];
			if (_firstLowerStackIndex <= i && !flag && UpperColumnHeight > 0f)
			{
				float num3 = UpperColumnHeight - TotalHeight + _filterSpacing * num2;
				if (i < _cardViews.Count - 1)
				{
					TotalHeight += Mathf.Abs(num3);
				}
				num -= num3;
				flag = true;
			}
			Vector3 vector = new Vector3(0f, num, 0f);
			if (staticColumnMetaCardView.RecentlyCreated)
			{
				staticColumnMetaCardView.transform.localPosition = vector;
				staticColumnMetaCardView.RecentlyCreated = false;
			}
			staticColumnMetaCardView.FrontOfColumn = i == _cardViews.Count - 1;
			if (staticColumnMetaCardView.Card.GrpId == _droppingGrpId)
			{
				staticColumnMetaCardView.transform.position = _droppingAtPosition;
				staticColumnMetaCardView.CardView.Root.localPosition = Vector3.zero;
			}
			if (staticColumnMetaCardView.transform.localPosition != vector)
			{
				staticColumnMetaCardView.transform.DOLocalMove(vector, RepositionMoveDuration).SetEase(Ease.InOutSine).SetTarget(this);
			}
			staticColumnMetaCardView.transform.localEulerAngles = CardStackRotation;
			float num4 = SpacingOffsetToUseForCard(assetLookupSystem, staticColumnMetaCardView.Card, staticColumnMetaCardView.Quantity, grpQuantityAdjust);
			num -= num4;
			if (i < _cardViews.Count - 1)
			{
				TotalHeight += Mathf.Abs(num4);
			}
		}
		TotalHeight += CardHeight;
		_droppingGrpId = 0u;
		_droppingAtPosition = Vector3.zero;
	}

	private float SpacingOffsetToUseForCard(AssetLookupSystem assetLookupSystem, CardData card, int cardQuantity, uint? grpQuantityAdjust)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataRaw(card);
		assetLookupSystem.Blackboard.IsExpanded = IsExpanded;
		Deckbuilder_ColumnStackOffsetOverrides payload = assetLookupSystem.TreeLoader.LoadTree<Deckbuilder_ColumnStackOffsetOverrides>().GetPayload(assetLookupSystem.Blackboard);
		if (card != null && card.GrpId == grpQuantityAdjust)
		{
			return payload.VerticalCardSpacingQuantityAdjust;
		}
		if (cardQuantity > 1)
		{
			return payload.VerticalCardSpacingMultiple;
		}
		return payload.VerticalCardSpacing;
	}

	public override DeckBuilderPile? GetParentDeckBuilderPile()
	{
		return DeckBuilderPile.MainDeck;
	}
}
