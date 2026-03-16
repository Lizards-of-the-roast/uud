using System;
using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using Assets.Core.Shared.Code.Utilities;
using MovementSystem;
using Pooling;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class StackCardHolder : ZoneCardHolderBase, IViewDismissBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider
{
	public enum DockStatus
	{
		NotDocked,
		Docked,
		AutoDocked
	}

	[SerializeField]
	private int _maxVisibleCards;

	[SerializeField]
	private int _showViewButtonCardCount;

	[SerializeField]
	protected Vector3 _endPosition;

	[SerializeField]
	protected CanvasGroup _canvasGroup;

	[SerializeField]
	private CustomButton _dockButton;

	[SerializeField]
	private Animator _dockButtonAnimator;

	[SerializeField]
	private Button _viewButton;

	[SerializeField]
	private TMP_Text _countLabel;

	[SerializeField]
	private bool CanHideEffectsRoot;

	protected DockStatus _dockStatus;

	protected CardLayout_HalfFan _fanLayout;

	private Camera _camera;

	private Transform _effectsRoot;

	private IObjectPool _genericPool;

	private Dictionary<string, ButtonStateData> browserButtonStateData;

	private ViewDismissBrowser openedBrowser;

	protected bool IsDocked
	{
		get
		{
			if (_dockStatus != DockStatus.Docked)
			{
				return _dockStatus == DockStatus.AutoDocked;
			}
			return true;
		}
	}

	public override Transform EffectsRoot => _effectsRoot;

	public uint TargetingSourceId
	{
		get
		{
			return _fanLayout.TargetingSourceId;
		}
		set
		{
			_fanLayout.TargetingSourceId = value;
			_isDirty = true;
		}
	}

	public string Header => Languages.ActiveLocProvider.GetLocalizedText("Enum/ZoneType/ZoneType_Stack");

	public string SubHeader => string.Empty;

	public Action<DuelScene_CDC> OnCardSelected => _gameManager.InteractionSystem.HandleViewDismissCardClick;

	public bool ApplyTargetOffset => false;

	public bool ApplySourceOffset => false;

	public bool ApplyControllerOffset => true;

	protected virtual void Awake()
	{
		_effectsRoot = new GameObject("EffectsRoot").transform;
		_effectsRoot.SetParent(base.transform);
		_effectsRoot.ZeroOut();
		_orderType = IdOrderType.Reversed;
		_fanLayout = new CardLayout_HalfFan();
		_fanLayout.HolderType = base.CardHolderType;
		_fanLayout.Radius = 5f;
		_fanLayout.OverlapOffset = 0.4f;
		_fanLayout.OverlapRotation = -5f;
		_fanLayout.TiltRatio = 1f;
		_fanLayout.PivotPercentage = 1f;
		_fanLayout.AdditionalRotation = 10f;
		_fanLayout.TotalDeltaAngle = 20f;
		base.Layout = _fanLayout;
		_countLabel.text = " ";
		_dockButton.gameObject.SetActive(value: false);
		_viewButton.gameObject.SetActive(value: false);
		_countLabel.gameObject.SetActive(value: false);
		_dockButton.OnClick.AddListener(OnDockButtonClicked);
		_viewButton.onClick.AddListener(ViewStack);
		browserButtonStateData = new Dictionary<string, ButtonStateData>();
		ButtonStateData buttonStateData = new ButtonStateData();
		buttonStateData.BrowserElementKey = "DismissButton";
		buttonStateData.LocalizedString = "DuelScene/Browsers/ViewDismiss_Done";
		buttonStateData.Enabled = true;
		buttonStateData.StyleType = ButtonStyle.StyleType.Main;
		browserButtonStateData.Add("DismissButton", buttonStateData);
	}

	public override void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		_camera = gameManager.MainCamera;
		_genericPool = gameManager.GenericPool;
		base.Init(gameManager, cardViewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
	}

	protected override void OnDestroy()
	{
		_dockButton.OnClick.RemoveAllListeners();
		_viewButton.onClick.RemoveAllListeners();
		base.OnDestroy();
	}

	protected void OnDockButtonClicked()
	{
		_dockButtonAnimator.SetBool("Pulse", value: false);
		Dock((!IsDocked) ? DockStatus.Docked : DockStatus.NotDocked);
	}

	public override void AddCard(DuelScene_CDC cardView)
	{
		if (openedBrowser != null && cardView.CurrentCardHolder.CardHolderType != CardHolderType.CardBrowserViewDismiss)
		{
			openedBrowser.Refresh();
			return;
		}
		base.AddCard(cardView);
		CardBrowserBase stackBrowser = openedBrowser;
		if (stackBrowser == null && _gameManager.CurrentInteraction is SelectTargetsWorkflow selectTargetsWorkflow)
		{
			stackBrowser = selectTargetsWorkflow.StackBrowser;
		}
		if (stackBrowser != null && stackBrowser.ReleasingCards)
		{
			_newlyAddedCards.Remove(cardView);
		}
		UpdateVfxOptionOnStack();
	}

	public override void RemoveCard(DuelScene_CDC cardView)
	{
		if (cardView.CollisionRoot != null)
		{
			cardView.CollisionRoot.gameObject.SetActive(value: true);
		}
		base.RemoveCard(cardView);
		UpdateVfxOptionOnStack();
	}

	private void UpdateVfxOptionOnStack()
	{
		TryGetTopCardOnStack(out var topCard);
		foreach (DuelScene_CDC cardView in base.CardViews)
		{
			foreach (Transform item in cardView.EffectsRoot)
			{
				VfxOptionsComponent component = item.GetComponent<VfxOptionsComponent>();
				if ((bool)component)
				{
					component.IsTopOfStack = topCard == cardView;
				}
			}
		}
	}

	private void UpdateButtons()
	{
		_dockButton.gameObject.UpdateActive(base.CardViews.Count > 0);
		if (_dockButtonAnimator.isActiveAndEnabled)
		{
			_dockButtonAnimator.SetBool("Docked", IsDocked);
		}
		bool flag = base.CardViews.Count >= _showViewButtonCardCount;
		if (_viewButton.gameObject.activeSelf != flag)
		{
			_viewButton.gameObject.SetActive(flag);
		}
		if (_countLabel.gameObject.activeSelf != flag)
		{
			_countLabel.gameObject.SetActive(flag);
		}
		_countLabel.text = base.CardViews.Count.ToString();
	}

	protected override void OnPreLayout()
	{
		if (base.CardViews.Count == 0 && IsDocked)
		{
			_dockStatus = DockStatus.NotDocked;
		}
		_fanLayout.Radius = (IsDocked ? 1f : 5f);
		UpdateButtons();
		base.OnPreLayout();
	}

	protected override void LayoutNowInternal(List<DuelScene_CDC> cardsToLayout, bool layoutInstantly = false)
	{
		_splineMovementSystem.AddPermanentGoal(_effectsRoot, new IdealPoint(base.transform.TransformPoint(GetLayoutCenterPoint()), base.transform.rotation, Vector3.one));
		if (cardsToLayout.Count > _maxVisibleCards)
		{
			List<DuelScene_CDC> range = cardsToLayout.GetRange(cardsToLayout.Count - _maxVisibleCards, _maxVisibleCards);
			List<DuelScene_CDC> range2 = cardsToLayout.GetRange(0, cardsToLayout.Count - _maxVisibleCards);
			_previousLayoutData.Clear();
			base.Layout.GenerateData(range, ref _previousLayoutData, GetLayoutCenterPoint(), GetLayoutRotation());
			for (int i = 0; i < _previousLayoutData.Count; i++)
			{
				CardLayoutData cardLayoutData = _previousLayoutData[i];
				cardLayoutData.Card.Root.parent = base.transform;
				ApplyLayoutData(cardLayoutData, _newlyAddedCards.Contains(cardLayoutData.Card), shouldBeVisible: true, layoutInstantly);
			}
			CardLayoutData cardLayoutData2 = _previousLayoutData[0];
			for (int num = range2.Count - 1; num >= 0; num--)
			{
				DuelScene_CDC duelScene_CDC = range2[num];
				CardLayoutData cardLayoutData3 = new CardLayoutData(duelScene_CDC, cardLayoutData2.Position, cardLayoutData2.Rotation, cardLayoutData2.Scale, isVisibleInLayout: false);
				_previousLayoutData.Insert(0, cardLayoutData3);
				duelScene_CDC.Root.parent = base.transform;
				ApplyLayoutData(cardLayoutData3, added: false, shouldBeVisible: false, layoutInstantly);
			}
			_newlyAddedCards.Clear();
		}
		else
		{
			base.LayoutNowInternal(cardsToLayout, layoutInstantly);
		}
	}

	protected override Vector3 GetLayoutCenterPoint()
	{
		if (!IsDocked)
		{
			return base.GetLayoutCenterPoint();
		}
		return _endPosition;
	}

	protected override bool CalcCardVisibility(CardLayoutData data, int indexInList)
	{
		bool flag;
		if (_gameManager.BrowserManager.IsBrowserVisible)
		{
			if (_gameManager.BrowserManager.CanShowStack)
			{
				flag = indexInList == _cardViews.Count - 1;
				SetEffectsRootVisibility(flag);
				return flag;
			}
			SetEffectsRootVisibility(visability: false);
			return false;
		}
		flag = indexInList >= _cardViews.Count - _maxVisibleCards;
		SetEffectsRootVisibility(flag);
		return flag;
	}

	private void SetEffectsRootVisibility(bool visability)
	{
		if (CanHideEffectsRoot)
		{
			EffectsRoot.gameObject.SetActive(visability);
		}
	}

	protected override SplineEventData GetLayoutSplineEvents(CardLayoutData data)
	{
		SplineEventData layoutSplineEvents = base.GetLayoutSplineEvents(data);
		layoutSplineEvents.Events.Add(new SplineEventAudio(0f, new List<AudioEvent>
		{
			new AudioEvent(WwiseEvents.sfx_basicloc_return_card.EventName)
		}, data.Card.Root.gameObject));
		return layoutSplineEvents;
	}

	public virtual void Dock(DockStatus dockStatus)
	{
		_dockStatus = dockStatus;
		UpdateButtons();
		LayoutNow();
	}

	public void ResetAutoDock()
	{
		if (_dockStatus == DockStatus.AutoDocked)
		{
			_dockButtonAnimator.SetBool("Pulse", value: false);
			Dock(DockStatus.NotDocked);
		}
	}

	public virtual void OnBrowserShown(BrowserBase browser)
	{
		if (!_gameManager.BrowserManager.CanShowStack)
		{
			foreach (DuelScene_CDC cardView in base.CardViews)
			{
				if (cardView.CollisionRoot != null)
				{
					cardView.CollisionRoot.gameObject.SetActive(value: false);
				}
			}
		}
		_canvasGroup.alpha = 0f;
		_canvasGroup.interactable = false;
		_dockStatus = DockStatus.NotDocked;
	}

	public virtual void OnBrowserHidden(BrowserBase browser)
	{
		if (!_gameManager.BrowserManager.CanShowStack)
		{
			foreach (DuelScene_CDC cardView in base.CardViews)
			{
				if (cardView.CollisionRoot != null)
				{
					cardView.CollisionRoot.gameObject.SetActive(value: true);
				}
			}
		}
		if ((bool)_canvasGroup)
		{
			_canvasGroup.alpha = 1f;
			_canvasGroup.interactable = true;
		}
	}

	public virtual void ViewStack()
	{
		IBrowser browser = _gameManager.BrowserManager.OpenBrowser(this);
		SetOpenedBrowser(browser);
	}

	public void TryAutoDock(IEnumerable<uint> selectableIds)
	{
		if (base.CardViews.Count == 0 || selectableIds == null)
		{
			return;
		}
		HashSet<DuelScene_CDC> hashSet = _genericPool.PopObject<HashSet<DuelScene_CDC>>();
		foreach (uint selectableId in selectableIds)
		{
			if (_cardViewProvider.TryGetCardView(selectableId, out var cardView))
			{
				if (base.CardViews.Contains(cardView))
				{
					hashSet.Clear();
					_genericPool.PushObject(hashSet, tryClear: false);
					return;
				}
				hashSet.Add(cardView);
			}
		}
		if (hashSet.Count == 0)
		{
			hashSet.Clear();
			_genericPool.PushObject(hashSet, tryClear: false);
			return;
		}
		Bounds bounds = base.CardViews[0].Collider.bounds;
		Vector3 center = bounds.center;
		bounds = GetStackBounds(bounds);
		Vector3 vector = bounds.center - center;
		bounds.center = base.transform.TransformPoint(GetLayoutCenterPoint()) + vector;
		Rect screenRect = bounds.GetScreenRect(_camera);
		foreach (DuelScene_CDC item in hashSet)
		{
			BoxCollider collider = item.Collider;
			if ((object)collider != null && DoesOverlap(screenRect, collider.bounds.size, item.Root, 0.5f))
			{
				_dockButtonAnimator.SetBool("Pulse", value: true);
				Dock(DockStatus.AutoDocked);
				break;
			}
		}
		hashSet.Clear();
		_genericPool.PushObject(hashSet, tryClear: false);
	}

	public void TryAutoDock(IReadOnlyList<ConfirmWidgetButton> buttonList)
	{
		if (base.CardViews.Count == 0 || buttonList.Count == 0)
		{
			return;
		}
		Bounds bounds = base.CardViews[0].Collider.bounds;
		Vector3 center = bounds.center;
		bounds = GetStackBounds(bounds);
		Vector3 vector = bounds.center - center;
		bounds.center = base.transform.TransformPoint(GetLayoutCenterPoint()) + vector;
		Rect screenRect = bounds.GetScreenRect(_camera);
		foreach (ConfirmWidgetButton button in buttonList)
		{
			Vector3 size = ((RectTransform)button.transform).GetBounds().size;
			if (DoesOverlap(screenRect, size, button.transform.parent, 0.35f))
			{
				_dockButtonAnimator.SetBool("Pulse", value: true);
				Dock(DockStatus.AutoDocked);
				break;
			}
		}
	}

	private bool DoesOverlap(Rect stackRect, Vector3 size, Transform root, float percent)
	{
		IdealPoint goal = _splineMovementSystem.GetGoal(root);
		Rect screenRect = new Bounds(goal.Position, Vector3.Scale(size, goal.Scale)).GetScreenRect(_camera);
		if (stackRect.Intersects(screenRect, out var area))
		{
			float num = area.width * area.height;
			float num2 = screenRect.width * screenRect.height;
			return ((num2 > 0f) ? (num / num2) : 0f) >= percent;
		}
		return false;
	}

	private Bounds GetStackBounds(Bounds stackBounds)
	{
		for (int i = 1; i < base.CardViews.Count; i++)
		{
			stackBounds.Encapsulate(base.CardViews[i].Collider.bounds);
		}
		return stackBounds;
	}

	public DuelScene_CDC GetTopCardOnStack()
	{
		if (!TryGetTopCardOnStack(out var topCard))
		{
			return null;
		}
		return topCard;
	}

	public bool TryGetTopCardOnStack(out DuelScene_CDC topCard)
	{
		topCard = null;
		if (base.CardViews.Count == 0)
		{
			return false;
		}
		DuelScene_CDC duelScene_CDC = base.CardViews[base.CardViews.Count - 1];
		if (duelScene_CDC == null || duelScene_CDC.Model == null)
		{
			return false;
		}
		topCard = duelScene_CDC;
		return true;
	}

	public Dictionary<string, ButtonStateData> GetButtonStateData()
	{
		return browserButtonStateData;
	}

	public void OnButtonPressed(string buttonKey)
	{
		if (buttonKey == "DismissButton")
		{
			openedBrowser.Close();
		}
	}

	public string GetCardHolderLayoutKey()
	{
		return "Stack";
	}

	public DuelSceneBrowserType GetBrowserType()
	{
		return DuelSceneBrowserType.ViewDismiss;
	}

	public List<DuelScene_CDC> GetCardsToDisplay()
	{
		List<DuelScene_CDC> list = new List<DuelScene_CDC>();
		if (_zoneModel != null)
		{
			foreach (uint cardId in _zoneModel.CardIds)
			{
				if (_cardViewProvider.TryGetCardView(cardId, out var cardView))
				{
					list.Add(cardView);
				}
			}
		}
		return list;
	}

	public BrowserCardHeader.BrowserCardHeaderData GetCardHeaderData(DuelScene_CDC cardView)
	{
		return null;
	}

	public void OnBrowserClosed()
	{
		openedBrowser.ClosedHandlers -= OnBrowserClosed;
		openedBrowser.CardViewRemovedHandlers -= OnCardRemovedFromBrowser;
		openedBrowser.ButtonPressedHandlers -= OnButtonPressed;
		openedBrowser = null;
	}

	public void SetOpenedBrowser(IBrowser openedBrowser)
	{
		this.openedBrowser = (ViewDismissBrowser)openedBrowser;
		this.openedBrowser.ClosedHandlers += OnBrowserClosed;
		this.openedBrowser.CardViewRemovedHandlers += OnCardRemovedFromBrowser;
		this.openedBrowser.ButtonPressedHandlers += OnButtonPressed;
	}

	public void OnCardRemovedFromBrowser(DuelScene_CDC duelScene_CDC)
	{
		if (!openedBrowser.ReleasingCards)
		{
			if (_zoneModel != null && _zoneModel.CardIds.Count == 0)
			{
				openedBrowser.Close();
			}
			else
			{
				openedBrowser.Refresh();
			}
		}
	}

	public void SetFxBlackboardData(IBlackboard bb)
	{
	}

	public void SetFxBlackboardDataForCard(DuelScene_CDC cardView, IBlackboard bb)
	{
	}
}
