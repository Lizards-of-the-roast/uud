using System;
using System.Collections;
using System.Collections.Generic;
using GreClient.Rules;
using MovementSystem;
using ReferenceMap;
using UnityEngine;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class CardBrowserCardHolder : CardHolderBase, ICardLock
{
	[SerializeField]
	private Vector3 _targetedPopoutAmount = Vector3.up;

	[SerializeField]
	private Vector3 _localPlayerOffset = Vector3.down;

	[SerializeField]
	private Vector3 _opponentOffset = Vector3.up;

	public bool ApplyTargetOffset { get; set; } = true;

	public bool ApplySourceOffset { get; set; }

	public bool ApplyControllerOffset { get; set; }

	public IGroupedCardProvider CardGroupProvider { get; set; }

	public bool LockCardDetails { get; set; }

	public event Action<List<CardLayoutData>> PostLayoutEvent;

	protected override void LateUpdate()
	{
		base.LateUpdate();
		ClampHoveredCardCollision();
	}

	protected override bool CalcCardVisibility(CardLayoutData data, int indexInList)
	{
		if (data.Card == CardDragController.DraggedCard)
		{
			return true;
		}
		return data.IsVisibleInLayout;
	}

	public int GetGroupIndexOfCardView(DuelScene_CDC cardView)
	{
		if (CardGroupProvider != null)
		{
			int num = 0;
			foreach (List<DuelScene_CDC> cardGroup in CardGroupProvider.GetCardGroups())
			{
				if (cardGroup.Contains(cardView))
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	public int GetIndexForCardInGroup(int groupIndex, DuelScene_CDC cardView)
	{
		return CardGroupProvider.GetCardGroups()[groupIndex].IndexOf(cardView);
	}

	public void SwapCardsInGroup(int groupIndex, int cardIndexOne, int cardIndexTwo)
	{
		List<List<DuelScene_CDC>> cardGroups = CardGroupProvider.GetCardGroups();
		List<DuelScene_CDC> cardGroup = cardGroups[groupIndex];
		DuelScene_CDC value = cardGroup[cardIndexOne];
		cardGroup[cardIndexOne] = cardGroup[cardIndexTwo];
		cardGroup[cardIndexTwo] = value;
		CardLayoutData cardLayoutData = _previousLayoutData.Find((CardLayoutData x) => x.Card == cardGroup[cardIndexOne]);
		CardLayoutData cardLayoutData2 = _previousLayoutData.Find((CardLayoutData x) => x.Card == cardGroup[cardIndexTwo]);
		cardLayoutData.Card = cardGroup[cardIndexTwo];
		cardLayoutData2.Card = cardGroup[cardIndexOne];
		IdealPoint layoutEndpoint = GetLayoutEndpoint(cardLayoutData);
		IdealPoint layoutEndpoint2 = GetLayoutEndpoint(cardLayoutData2);
		_splineMovementSystem.AddPermanentGoal(cardLayoutData.Card.Root, layoutEndpoint);
		_splineMovementSystem.AddPermanentGoal(cardLayoutData2.Card.Root, layoutEndpoint2);
	}

	public void ShiftCardsInGroup(int groupIndex, int cardIndex, int targetIndex)
	{
		while (cardIndex < targetIndex)
		{
			SwapCardsInGroup(groupIndex, cardIndex, cardIndex + 1);
			cardIndex++;
		}
		while (cardIndex > targetIndex)
		{
			SwapCardsInGroup(groupIndex, cardIndex, cardIndex - 1);
			cardIndex--;
		}
	}

	public int GetClosestCardIndexToPositionInGroup(int groupIndex, float cardLocalX)
	{
		List<List<DuelScene_CDC>> cardGroups = CardGroupProvider.GetCardGroups();
		List<DuelScene_CDC> cardGroup = cardGroups[groupIndex];
		float num = float.MaxValue;
		int result = -1;
		int i = 0;
		while (i < cardGroup.Count)
		{
			if (!base.IgnoreDummyCards || cardGroup[i].Model.InstanceId != 0)
			{
				CardLayoutData cardLayoutData = _previousLayoutData.Find((CardLayoutData x) => x.Card == cardGroup[i]);
				if (cardLayoutData != null)
				{
					float num2 = Mathf.Abs(cardLayoutData.Position.x - cardLocalX);
					if (num2 < num)
					{
						num = num2;
						result = i;
					}
				}
			}
			int num3 = i + 1;
			i = num3;
		}
		return result;
	}

	public override void SwapCards(int cardIndexOne, int cardIndexTwo)
	{
		if (CardGroupProvider == null)
		{
			base.SwapCards(cardIndexOne, cardIndexTwo);
		}
	}

	public override void ShiftCards(int cardIndex, int targetIndex)
	{
		if (CardGroupProvider == null)
		{
			base.ShiftCards(cardIndex, targetIndex);
		}
	}

	protected override void OnPreLayout()
	{
		SortedBrowser()?.Sort(_cardViews);
		if (_gameManager.BrowserManager.CurrentBrowser is OpeningHandBrowser openingHandBrowser)
		{
			openingHandBrowser.SetOpeningHandText();
		}
		base.OnPreLayout();
	}

	private ISortedBrowser SortedBrowser()
	{
		if (_gameManager == null)
		{
			return null;
		}
		if (_gameManager.BrowserManager == null)
		{
			return null;
		}
		return _gameManager.BrowserManager.CurrentBrowser as ISortedBrowser;
	}

	protected override void OnPostLayout()
	{
		base.OnPostLayout();
		this.PostLayoutEvent?.Invoke(_previousLayoutData);
	}

	protected override Vector3 GetLayoutCenterPoint()
	{
		return Vector3.zero + _safeAreaOffset;
	}

	protected override void ApplyLayoutData(CardLayoutData data, bool added, bool shouldBeVisible, bool moveInstantly = false)
	{
		bool flag = false;
		if (ApplyTargetOffset && TargetedBySomethingOnStack())
		{
			flag = true;
		}
		if (ApplySourceOffset && sourceOfSomethingOnStack(data, _gameManager))
		{
			flag = true;
		}
		if (flag)
		{
			data.Position += _targetedPopoutAmount;
		}
		if (ApplyControllerOffset)
		{
			Vector3 vector = _opponentOffset;
			if (data.Card != null && data.Card.Model != null && data.Card.Model.Controller.IsLocalPlayer)
			{
				vector = _localPlayerOffset;
			}
			data.Position += vector;
		}
		if (LockCardDetails)
		{
			moveInstantly = false;
		}
		base.ApplyLayoutData(data, added, shouldBeVisible, moveInstantly);
		bool TargetedBySomethingOnStack()
		{
			if (data.Card == null)
			{
				return false;
			}
			if (data.Card.Model == null)
			{
				return false;
			}
			foreach (MtgEntity item in data.Card.Model.TargetedBy)
			{
				if (item is MtgCardInstance { Zone: not null } mtgCardInstance && mtgCardInstance.Zone.Type == ZoneType.Stack)
				{
					return true;
				}
			}
			return false;
		}
		static bool isAbilityOnTheStack(MtgCardInstance c)
		{
			if (c != null && c.ObjectType == GameObjectType.Ability && c.Zone != null)
			{
				return c.Zone.Type == ZoneType.Stack;
			}
			return false;
		}
		bool sourceOfSomethingOnStack(CardLayoutData d, GameManager g)
		{
			if (!d.Card)
			{
				return false;
			}
			if (d.Card.Model == null)
			{
				return false;
			}
			foreach (MtgCardInstance child in d.Card.Model.Instance.Children)
			{
				if (isAbilityOnTheStack(child))
				{
					return true;
				}
			}
			if (g.LatestGameState != null)
			{
				HashSet<ReferenceMap.Reference> results = _gameManager.GenericPool.PopObject<HashSet<ReferenceMap.Reference>>();
				g.LatestGameState.ReferenceMap.GetReferences(0u, ReferenceMap.ReferenceType.Triggered, 0u, ref results);
				foreach (ReferenceMap.Reference item2 in results)
				{
					if (item2.A == d.Card.InstanceId && g.LatestGameState.GetEntityById(item2.B) is MtgCardInstance c && isAbilityOnTheStack(c))
					{
						_gameManager.GenericPool.PushObject(results);
						return true;
					}
				}
				results.Clear();
				_gameManager.GenericPool.PushObject(results, tryClear: false);
			}
			return false;
		}
	}

	public override void RemoveCard(DuelScene_CDC cardView)
	{
		base.RemoveCard(cardView);
		if (LockCardDetails)
		{
			cardView.HolderTypeOverride = null;
			cardView.UpdateVisuals();
		}
	}

	protected override SplineEventData GetLayoutSplineEvents(CardLayoutData data)
	{
		return new SplineEventData();
	}

	public IEnumerator ClearLockCardDetails()
	{
		yield return new WaitForEndOfFrame();
		yield return new WaitForEndOfFrame();
		LockCardDetails = false;
	}
}
