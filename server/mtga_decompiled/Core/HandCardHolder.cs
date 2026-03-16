using System;
using System.Collections.Generic;
using MovementSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class HandCardHolder : BaseHandCardHolder, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private const float DROP_FOR_DRAG_SPEED = 3f;

	[SerializeField]
	private MaxHandSizeView _maxHandSize;

	[SerializeField]
	protected float _dropForDragZOffset = 2f;

	[SerializeField]
	protected float _dropForDragYOffset;

	[SerializeField]
	protected CardLayout_Hand _expandedFanLayout;

	private float _previousFOV;

	protected bool _dropHandForDrag;

	public event Action<ZoneType, GREPlayerNum> Hovered;

	public override void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		base.Init(gameManager, cardViewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
		if (_gameManager.NpeDirector != null)
		{
			UnityEngine.Object.Destroy(_maxHandSize.gameObject);
			_maxHandSize = null;
		}
		else
		{
			_maxHandSize.Init(gameManager.MainCamera);
		}
	}

	protected override void LateUpdate()
	{
		base.LateUpdate();
		ClampHoveredCardCollision();
	}

	protected override void Awake()
	{
		SetHandLayout();
	}

	private void SetHandLayout()
	{
		CardLayout_Hand handLayout = new CardLayout_Hand(_expandedFanLayout);
		base.Layout = (_handLayout = handLayout);
		LayoutNow();
	}

	public virtual void DropHandForDrag(bool dropHand)
	{
		if (_dropHandForDrag != dropHand)
		{
			_dropHandForDrag = dropHand;
			_handLayout.ZOffset = (_dropHandForDrag ? _dropForDragZOffset : _expandedFanLayout.ZOffset);
			_handLayout.YOffset = (_dropHandForDrag ? _dropForDragYOffset : _expandedFanLayout.YOffset);
			_handLayout.Radius = _expandedFanLayout.Radius;
			LayoutNow();
		}
	}

	protected override void HandleAddedCard(DuelScene_CDC cardView)
	{
		base.HandleAddedCard(cardView);
		OnHandCountUpdated();
		SetHandManaCostSorting();
	}

	public override void RemoveCard(DuelScene_CDC cardView)
	{
		if (_cardViews.Contains(cardView))
		{
			base.RemoveCard(cardView);
			OnHandCountUpdated();
			SetHandManaCostSorting();
			cardView.TrySetManaCostSortingOrder(0);
			cardView.SetUIClickGuardEnabled(enabled: false);
		}
	}

	public void SetMaxHandSize(uint count)
	{
		_maxHandSize?.SetMaxHandCount(count);
	}

	public virtual void OnTurnChange(GREPlayerNum activePlayer)
	{
		_maxHandSize?.OnTurnChange(activePlayer);
	}

	private void OnHandCountUpdated()
	{
		List<DuelScene_CDC> list = _cardViews.FindAll((DuelScene_CDC x) => x.Model.Instance.Zone.Type == ZoneType.Hand);
		_maxHandSize?.SetCurrentHandCount(list.Count);
	}

	public override void ShiftCards(int cardIndex, int targetIndex)
	{
		cardIndex = Mathf.Clamp(cardIndex, 0, _cardViews.Count - 1);
		targetIndex = Mathf.Clamp(targetIndex, 0, _cardViews.Count - 1);
		while (cardIndex < targetIndex)
		{
			DuelScene_CDC duelScene_CDC = _cardViews[cardIndex];
			DuelScene_CDC duelScene_CDC2 = _cardViews[cardIndex + 1];
			if (duelScene_CDC.Model.Zone.Type != duelScene_CDC2.Model.Zone.Type)
			{
				break;
			}
			SwapCards(cardIndex, cardIndex + 1);
			cardIndex++;
		}
		while (cardIndex > targetIndex)
		{
			DuelScene_CDC duelScene_CDC3 = _cardViews[cardIndex];
			DuelScene_CDC duelScene_CDC4 = _cardViews[cardIndex - 1];
			if (duelScene_CDC3.Model.Zone.Type != duelScene_CDC4.Model.Zone.Type)
			{
				break;
			}
			SwapCards(cardIndex, cardIndex - 1);
			cardIndex--;
		}
		SetHandManaCostSorting();
	}

	public override IdealPoint GetLayoutEndpoint(CardLayoutData data)
	{
		IdealPoint layoutEndpoint = base.GetLayoutEndpoint(data);
		layoutEndpoint.Speed = (_dropHandForDrag ? 3f : 1f);
		return layoutEndpoint;
	}

	protected override SplineEventData GetLayoutSplineEvents(CardLayoutData data)
	{
		SplineEventData layoutSplineEvents = base.GetLayoutSplineEvents(data);
		layoutSplineEvents.Events.Add(new SplineEventAudio(0f, new List<AudioEvent>
		{
			new AudioEvent(WwiseEvents.sfx_basicloc_draw_card_1st.EventName)
		}, data.CardGameObject));
		layoutSplineEvents.Events.Add(new SplineEventAudio(0.8f, new List<AudioEvent>
		{
			new AudioEvent(WwiseEvents.sfx_basicloc_draw_card_2nd.EventName)
		}, data.CardGameObject));
		return layoutSplineEvents;
	}

	public void OnLanguageChanged()
	{
		_maxHandSize?.OnLanguageChanged();
	}

	void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
	{
		OnPointerEnter(eventData);
	}

	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		ZoneType arg = ZoneType.None;
		Transform eventTran = null;
		if (eventData?.pointerEnter != null)
		{
			eventTran = eventData.pointerEnter.transform;
		}
		else if (eventData?.pointerPress != null)
		{
			eventTran = eventData.pointerPress.transform;
		}
		if ((bool)eventTran)
		{
			DuelScene_CDC duelScene_CDC = _cardViews.Find((DuelScene_CDC x) => x.CollisionRoot == eventTran);
			if ((bool)duelScene_CDC && duelScene_CDC.Model != null && duelScene_CDC.Model.ZoneType != ZoneType.Hand)
			{
				arg = duelScene_CDC.Model.ZoneType;
			}
		}
		this.Hovered?.Invoke(arg, base.PlayerNum);
	}

	void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
	{
		OnPointerExit();
	}

	public virtual void OnPointerExit()
	{
		this.Hovered?.Invoke(ZoneType.None, base.PlayerNum);
	}

	public virtual void OnLifePillClicked()
	{
	}

	public override void HandleClick(PointerEventData eventData, CardInput cardInput)
	{
		if (cardInput != null && CardHandlesInput())
		{
			cardInput.HandleClick(eventData);
		}
	}

	public virtual void SetHandCollapse(bool collapsed)
	{
	}

	public void SetHandManaCostSorting()
	{
		int num = 1;
		foreach (DuelScene_CDC cardView in _cardViews)
		{
			if (cardView.TrySetManaCostSortingOrder(num))
			{
				num++;
			}
		}
	}

	public void ResetHandManaCostSorting()
	{
		foreach (DuelScene_CDC cardView in _cardViews)
		{
			cardView.TrySetManaCostSortingOrder(0);
		}
	}

	public override bool CardHandlesInput()
	{
		return true;
	}

	protected override void OnDestroy()
	{
		this.Hovered = null;
		base.OnDestroy();
	}
}
