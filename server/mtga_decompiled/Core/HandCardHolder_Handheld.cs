using System.Collections.Generic;
using MovementSystem;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HandCardHolder_Handheld : HandCardHolder
{
	[SerializeField]
	private CardLayout_Hand _collapsedFanLayout;

	[SerializeField]
	private GameObject _clickBlocker;

	[SerializeField]
	private bool _collapsable;

	[SerializeField]
	private Scrollbar _scrollBar;

	private bool _handCollapsed;

	private bool _pointerInside;

	private bool _lifePillClicked;

	private int _laidOutCardCount;

	public bool LayoutComplete { get; private set; }

	protected override void Awake()
	{
		if (_collapsable)
		{
			ToggleHandCollapse(collapsed: false);
		}
		SetHandLayout();
	}

	protected virtual void Start()
	{
		if (!_scrollBar)
		{
			return;
		}
		Transform parent = _scrollBar.transform.parent;
		if ((object)parent != null)
		{
			Canvas component = parent.GetComponent<Canvas>();
			if ((object)component != null)
			{
				component.worldCamera = _gameManager.MainCamera;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (_collapsable && !_handCollapsed && Input.GetMouseButtonDown(0) && !_pointerInside && !_gameManager.BrowserManager.IsAnyBrowserOpen)
		{
			ToggleHandCollapse(collapsed: true);
			SetHandManaCostSorting();
		}
		if (_lifePillClicked)
		{
			if (_collapsable)
			{
				ToggleHandCollapse(collapsed: false);
			}
			_lifePillClicked = false;
		}
	}

	protected override void SetFocusPosition()
	{
		if (!_scrollBar)
		{
			return;
		}
		if (_cardViews.Count >= _handLayout.MinimumCardsForFocus && !_handCollapsed && !_dropHandForDrag)
		{
			float value = _scrollBar.value;
			Vector3 value2 = new Vector3(value * (_handLayout.GetRightmostmostX - _handLayout.GetLeftmostX) + _handLayout.GetLeftmostX, 0f, 0f);
			if (_handLayout.SetFocusPosition(value2))
			{
				LayoutNow();
			}
			_scrollBar.gameObject.SetActive(!_gameManager.InteractionSystem.HoverController.IsHovering);
		}
		else
		{
			_scrollBar.gameObject.SetActive(value: false);
		}
	}

	private void SetHandLayout()
	{
		CardLayout_Hand handLayout = new CardLayout_Hand(_handCollapsed ? _collapsedFanLayout : _expandedFanLayout);
		base.Layout = (_handLayout = handLayout);
		LayoutNow();
	}

	protected void ToggleHandCollapse(bool collapsed)
	{
		_handCollapsed = collapsed;
		if ((bool)_clickBlocker)
		{
			_clickBlocker.SetActive(!collapsed);
			ToggleCardsUIClickGuard(!collapsed);
		}
		SetHandLayout();
		SetHandManaCostSorting();
	}

	private void ToggleCardsUIClickGuard(bool active)
	{
		foreach (DuelScene_CDC cardView in base.CardViews)
		{
			cardView.SetUIClickGuardEnabled(active);
		}
	}

	protected override void LayoutNowInternal(List<DuelScene_CDC> cardsToLayout, bool layoutInstantly = false)
	{
		LayoutComplete = layoutInstantly;
		base.LayoutNowInternal(cardsToLayout, layoutInstantly);
	}

	protected override void MoveSplineEvents(SplineEventData events, CardLayoutData data)
	{
		events.Events.Add(new SplineEventCallbackWithParams<(DuelScene_CDC, HandCardHolder)>(1f, (data.Card, this), delegate(float prog, (DuelScene_CDC, HandCardHolder) param)
		{
			var (duelScene_CDC, _) = param;
			if ((object)duelScene_CDC != null && param.Item2 is HandCardHolder_Handheld handCardHolder_Handheld && prog >= 1f && (CardHolderBase)duelScene_CDC.CurrentCardHolder == handCardHolder_Handheld)
			{
				handCardHolder_Handheld._laidOutCardCount++;
				if (handCardHolder_Handheld._laidOutCardCount >= handCardHolder_Handheld._cardViews.Count)
				{
					handCardHolder_Handheld.LayoutComplete = true;
					handCardHolder_Handheld._laidOutCardCount = 0;
				}
			}
		}));
	}

	public override void DropHandForDrag(bool dropHand)
	{
		if (_dropHandForDrag != dropHand)
		{
			_dropHandForDrag = dropHand;
			_handLayout.ZOffset = (_dropHandForDrag ? _dropForDragZOffset : (_handCollapsed ? _collapsedFanLayout.ZOffset : _expandedFanLayout.ZOffset));
			_handLayout.YOffset = (_dropHandForDrag ? _dropForDragYOffset : (_handCollapsed ? _collapsedFanLayout.YOffset : _expandedFanLayout.YOffset));
			_handLayout.Radius = ((_dropHandForDrag && _collapsable) ? _collapsedFanLayout.Radius : _expandedFanLayout.Radius);
			LayoutNow();
		}
	}

	protected override void HandleAddedCard(DuelScene_CDC cardView)
	{
		if (_collapsable)
		{
			ToggleHandCollapse(collapsed: false);
		}
		base.HandleAddedCard(cardView);
	}

	public override void OnTurnChange(GREPlayerNum activePlayer)
	{
		base.OnTurnChange(activePlayer);
		if (_collapsable)
		{
			ToggleHandCollapse(activePlayer == GREPlayerNum.Opponent);
		}
	}

	public override void OnPointerEnter(PointerEventData eventData)
	{
		_pointerInside = true;
		base.OnPointerEnter(eventData);
	}

	public override void OnPointerExit()
	{
		_pointerInside = false;
		base.OnPointerExit();
	}

	public override void OnLifePillClicked()
	{
		base.OnLifePillClicked();
		if (_handCollapsed)
		{
			_lifePillClicked = true;
		}
	}

	public override void HandleClick(PointerEventData eventData, CardInput cardInput)
	{
		if ((bool)cardInput && CardHandlesInput())
		{
			cardInput.HandleClick(eventData);
		}
		else
		{
			ToggleHandCollapse(collapsed: false);
		}
	}

	public override void SetHandCollapse(bool collapsed)
	{
		base.SetHandCollapse(collapsed);
		if (_collapsable)
		{
			ToggleHandCollapse(collapsed);
		}
	}

	public override bool CardHandlesInput()
	{
		if (_collapsable)
		{
			return !_handCollapsed;
		}
		return true;
	}
}
