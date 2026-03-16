using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using MovementSystem;
using Pooling;
using UnityEngine;
using UnityEngine.EventSystems;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class CardDragController : IDisposable, IUpdate
{
	private const float CastFromHandYThresholdScreenPercentage = 0.3f;

	private const float DefaultDragOffsetMultiplier = 3f;

	private readonly IObjectPool _objectPool;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly CardViewBuilder _cardViewBuilder;

	private readonly Camera _camera;

	private readonly CanvasManager _canvasManager;

	private readonly ICardHolderProvider _cardHolderProvider;

	private readonly IBrowserProvider _browserProvider;

	private readonly HangerController _hangerController;

	private readonly ISplineMovementSystem _splineMovementSystem;

	private HandCardHolder _localHandCache;

	private DuelScene_CDC _draggedCard;

	private float _dragZ;

	private Vector3 _dragOffset = Vector3.one;

	private Vector2 _angle = Vector2.zero;

	private Vector2 _prevScreenPosition = Vector3.zero;

	private HandCardHolder _localHand => _localHandCache ?? (_localHandCache = _cardHolderProvider.GetCardHolder<HandCardHolder>(GREPlayerNum.LocalPlayer, CardHolderType.Hand));

	public static DuelScene_CDC DraggedCard { get; private set; }

	public bool IsDragging { get; private set; }

	private bool IsDraggedCardMoved { get; set; }

	private static Vector2 MousePointScreen => UnityEngine.Input.mousePosition;

	private Vector3 MousePointWorld => _camera.ScreenToWorldPoint(new Vector3(UnityEngine.Input.mousePosition.x, UnityEngine.Input.mousePosition.y, _dragZ));

	public event Action<DuelScene_CDC> DraggedCardUpdated;

	public event Action<DuelScene_CDC> CardDragStarted;

	public event Action<DuelScene_CDC> CardDragEnded;

	public DuelScene_CDC GetDraggedCard()
	{
		return _draggedCard;
	}

	public CardDragController(IObjectPool objectPool, AssetLookupSystem assetLookupSystem, CardViewBuilder cardViewBuilder, Camera camera, CanvasManager canvasManager, ICardHolderProvider cardHolderProvider, IBrowserProvider browserProvider, HangerController hangerController, ISplineMovementSystem splineMovementSystem)
	{
		_objectPool = objectPool ?? NullObjectPool.Default;
		_assetLookupSystem = assetLookupSystem;
		_cardViewBuilder = cardViewBuilder;
		_camera = camera;
		_canvasManager = canvasManager;
		_cardHolderProvider = cardHolderProvider ?? NullCardHolderProvider.Default;
		_browserProvider = browserProvider ?? NullBrowserProvider.Default;
		_splineMovementSystem = splineMovementSystem;
		_cardViewBuilder = cardViewBuilder;
		_hangerController = hangerController;
		_cardViewBuilder.preCardDestroyEvent += HandleCardDestroy;
	}

	public void BeginDrag(DuelScene_CDC cardView)
	{
		if (DraggedCard == cardView)
		{
			return;
		}
		if (DraggedCard != null)
		{
			EndDrag();
		}
		DraggedCard = (_draggedCard = cardView);
		IsDragging = true;
		IsDraggedCardMoved = false;
		this.DraggedCardUpdated?.Invoke(DraggedCard);
		this.CardDragStarted?.Invoke(DraggedCard);
		if (DraggedCard.CurrentCardHolder.CardHolderType == CardHolderType.Library)
		{
			Vector3? obj = _localHand?.CardRoot?.position;
			_dragZ = (_camera.transform.position - (obj ?? DraggedCard.Root.position)).magnitude;
		}
		else
		{
			_dragZ = (_camera.transform.position - DraggedCard.CurrentCardHolder.CardRoot.position).magnitude;
		}
		_dragOffset = (_camera.transform.position - MousePointWorld).normalized;
		_prevScreenPosition = MousePointScreen;
		IdealPoint endPoint = new IdealPoint(DraggedCard.Root);
		endPoint.Position = MousePointWorld - _dragOffset * 3f;
		endPoint.Speed = 5f;
		switch (DraggedCard.CurrentCardHolder.CardHolderType)
		{
		case CardHolderType.CardBrowserDefault:
			_splineMovementSystem.AddTemporaryGoal(DraggedCard.Root, endPoint, allowInteractions: true);
			break;
		case CardHolderType.Library:
			playDragBeginSfx();
			_splineMovementSystem.AddTemporaryGoal(DraggedCard.Root, endPoint, allowInteractions: true);
			break;
		case CardHolderType.Hand:
			if (!PlatformUtils.IsHandheld() || IsMousePointAboveThreshold())
			{
				BeginDragCardInHand();
				_splineMovementSystem.AddTemporaryGoal(DraggedCard.Root, endPoint, allowInteractions: true);
			}
			break;
		}
		_canvasManager.SetCanvasInputEnabled(enabled: false);
	}

	public void OnUpdate(float deltaTime)
	{
		if (!IsDragging)
		{
			return;
		}
		if (PlatformUtils.IsHandheld())
		{
			if (!IsMousePointAboveThreshold() && !IsDraggedCardMoved)
			{
				return;
			}
			if (!IsDraggedCardMoved)
			{
				BeginDragCardInHand();
			}
		}
		IsDraggedCardMoved = true;
		bool flag = false;
		_dragOffset = (_camera.transform.position - MousePointWorld).normalized;
		IdealPoint endPoint = new IdealPoint(DraggedCard.Root);
		endPoint.Position = MousePointWorld + _dragOffset * 3f;
		endPoint.Speed = 5f;
		switch (DraggedCard.CurrentCardHolder.CardHolderType)
		{
		case CardHolderType.CardBrowserDefault:
			flag = true;
			endPoint.Scale = Vector3.one * 0.75f;
			if (_browserProvider.CurrentBrowser is CardBrowserBase cardBrowserBase)
			{
				cardBrowserBase.HandleDrag(DraggedCard);
			}
			_splineMovementSystem.AddTemporaryGoal(DraggedCard.Root, endPoint);
			break;
		case CardHolderType.Hand:
		{
			Vector2 vector = MousePointScreen - _prevScreenPosition;
			float value = vector.magnitude / (float)Screen.width * 100f;
			AudioManager.SetRTPCValue("card_speed", value);
			_angle += vector * 1f;
			_angle = Vector2.Lerp(_angle, Vector2.zero, 5f * Time.deltaTime);
			_angle.x = Mathf.Clamp(_angle.x, -15f, 15f);
			_angle.y = Mathf.Clamp(_angle.y, -15f, 15f);
			Vector2 vector2 = new Vector3(_angle.y, 0f - _angle.x);
			Vector3 localEulerAngles = DraggedCard.Root.localEulerAngles;
			DraggedCard.Root.localEulerAngles = vector2;
			endPoint.Rotation = DraggedCard.Root.rotation;
			DraggedCard.Root.localEulerAngles = localEulerAngles;
			endPoint.Scale = Vector3.one * 1.5f;
			if (!IsDraggedCardAboveThreshold())
			{
				flag = true;
			}
			_splineMovementSystem.AddTemporaryGoal(DraggedCard.Root, endPoint);
			break;
		}
		case CardHolderType.Library:
			_splineMovementSystem.AddTemporaryGoal(DraggedCard.Root, endPoint);
			break;
		}
		if (IsScreenPointAboveThreshold(_prevScreenPosition) != IsScreenPointAboveThreshold(MousePointScreen))
		{
			this.DraggedCardUpdated?.Invoke(DraggedCard);
		}
		_prevScreenPosition = MousePointScreen;
		if (!flag)
		{
			return;
		}
		CardBrowserCardHolder cardBrowserCardHolder = DraggedCard.CurrentCardHolder as CardBrowserCardHolder;
		if (cardBrowserCardHolder != null && cardBrowserCardHolder.CardGroupProvider != null)
		{
			int groupIndexOfCardView = cardBrowserCardHolder.GetGroupIndexOfCardView(DraggedCard);
			int indexForCardInGroup = cardBrowserCardHolder.GetIndexForCardInGroup(groupIndexOfCardView, DraggedCard);
			int closestCardIndexToPositionInGroup = cardBrowserCardHolder.GetClosestCardIndexToPositionInGroup(groupIndexOfCardView, DraggedCard.Root.localPosition.x);
			if (closestCardIndexToPositionInGroup != indexForCardInGroup && closestCardIndexToPositionInGroup >= 0 && indexForCardInGroup >= 0)
			{
				cardBrowserCardHolder.ShiftCardsInGroup(groupIndexOfCardView, indexForCardInGroup, closestCardIndexToPositionInGroup);
			}
		}
		else
		{
			int indexForCard = DraggedCard.CurrentCardHolder.GetIndexForCard(DraggedCard);
			int closestCardIndexToPosition = DraggedCard.CurrentCardHolder.GetClosestCardIndexToPosition(DraggedCard.Root.localPosition.x);
			if (closestCardIndexToPosition != indexForCard)
			{
				DraggedCard.CurrentCardHolder.ShiftCards(indexForCard, closestCardIndexToPosition);
			}
		}
	}

	public void EndDrag()
	{
		if (!IsDragging)
		{
			return;
		}
		_splineMovementSystem.RemoveTemporaryGoal(DraggedCard.Root);
		DraggedCard.ClearOverrides();
		switch (DraggedCard.CurrentCardHolder.CardHolderType)
		{
		case CardHolderType.Hand:
		{
			HandCardHolder handCardHolder = DraggedCard.CurrentCardHolder as HandCardHolder;
			if (handCardHolder != null)
			{
				handCardHolder.DropHandForDrag(dropHand: false);
				((IPointerExitHandler)handCardHolder).OnPointerExit(new PointerEventData(EventSystem.current)
				{
					selectedObject = DraggedCard.gameObject
				});
			}
			AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_return_card, DraggedCard.Root.gameObject);
			break;
		}
		case CardHolderType.CardBrowserDefault:
			if (_browserProvider.CurrentBrowser is CardBrowserBase cardBrowserBase)
			{
				cardBrowserBase.OnDragRelease(DraggedCard);
			}
			break;
		}
		this.CardDragEnded?.Invoke(DraggedCard);
		_canvasManager.SetCanvasInputEnabled(enabled: true);
		bool num = DraggedCard != null;
		IsDragging = false;
		DraggedCard = (_draggedCard = null);
		IsDraggedCardMoved = false;
		_hangerController.ClearHangers();
		if (num)
		{
			this.DraggedCardUpdated?.Invoke(null);
		}
	}

	private bool IsDraggedCardAboveThreshold()
	{
		if (DraggedCard == null)
		{
			return false;
		}
		return IsScreenPointAboveThreshold(_camera.WorldToScreenPoint(DraggedCard.Root.position));
	}

	public bool IsCardAboveThreshold(DuelScene_CDC cardView)
	{
		if (cardView == null || cardView.Root == null)
		{
			return false;
		}
		return IsScreenPointAboveThreshold(_camera.WorldToScreenPoint(cardView.Root.position));
	}

	public bool IsDragPointAboveThreshold()
	{
		if (DraggedCard == null)
		{
			return false;
		}
		return IsScreenPointAboveThreshold(MousePointScreen);
	}

	public bool IsMousePointAboveThreshold()
	{
		return IsScreenPointAboveThreshold(MousePointScreen);
	}

	private bool IsScreenPointAboveThreshold(Vector2 screenPoint)
	{
		float num = (float)Screen.height * 0.3f;
		return screenPoint.y >= num;
	}

	private void HandleCardDestroy(BASE_CDC baseCDC)
	{
		if ((bool)baseCDC && baseCDC.Model != null)
		{
			GameObjectType objectType = baseCDC.Model.ObjectType;
			if (objectType != GameObjectType.SplitLeft && objectType != GameObjectType.SplitRight && !(DraggedCard == null) && !(DraggedCard != baseCDC))
			{
				EndDrag();
			}
		}
	}

	private void playDragBeginSfx()
	{
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(DraggedCard.Model);
		_assetLookupSystem.Blackboard.CardHolderType = DraggedCard.CurrentCardHolder.CardHolderType;
		AssetLookupTree<DragSFX> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<DragSFX>();
		HashSet<DragSFX> hashSet = _objectPool.PopObject<HashSet<DragSFX>>();
		assetLookupTree.GetPayloadLayered(_assetLookupSystem.Blackboard, hashSet);
		foreach (DragSFX item in hashSet)
		{
			AudioManager.PlayAudio(item.SfxData.AudioEvents, DraggedCard.gameObject);
		}
		hashSet.Clear();
		_objectPool.PushObject(hashSet, tryClear: false);
	}

	private void BeginDragCardInHand()
	{
		HandCardHolder handCardHolder = DraggedCard.CurrentCardHolder as HandCardHolder;
		if (!(handCardHolder == null))
		{
			_hangerController?.ClearHangers();
			handCardHolder.DropHandForDrag(dropHand: true);
			DraggedCard.IsMousedOver = true;
			playDragBeginSfx();
		}
	}

	public void Dispose()
	{
		_cardViewBuilder.preCardDestroyEvent -= HandleCardDestroy;
		this.DraggedCardUpdated = null;
		this.CardDragStarted = null;
		this.CardDragEnded = null;
		DraggedCard = (_draggedCard = null);
		_localHandCache = null;
	}
}
