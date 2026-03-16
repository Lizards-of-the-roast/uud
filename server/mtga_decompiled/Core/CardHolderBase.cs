using System;
using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using MovementSystem;
using Pooling;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Duel;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public abstract class CardHolderBase : MonoBehaviour, ICardHolder
{
	public enum CardPosition
	{
		None,
		Top,
		Middle,
		Bottom
	}

	protected GameManager _gameManager;

	protected AssetLookupSystem _assetLookupSystem;

	protected IVfxProvider _vfxProvider = NullVfxProvider.Default;

	protected ICardViewProvider _cardViewProvider = NullCardViewProvider.Default;

	protected IClientLocProvider _locManager = NullLocProvider.Default;

	protected ISplineMovementSystem _splineMovementSystem;

	protected CardViewBuilder _cardViewBuilder;

	protected MatchManager _matchManager;

	[SerializeField]
	public bool UseSecondaryLayout;

	[SerializeField]
	public SecondaryLayoutContainer SecondaryLayoutContainer;

	[SerializeField]
	protected GREPlayerNum playerNum;

	[SerializeField]
	protected CardHolderType m_cardHolderType;

	[SerializeField]
	protected bool _overrideCdcSize;

	[SerializeField]
	protected float _cdcSize = 1f;

	[SerializeField]
	protected bool _enableShadows;

	protected List<DuelScene_CDC> _cardViews = new List<DuelScene_CDC>();

	protected bool _isDirty;

	protected bool _isUpdated;

	protected readonly HashSet<DuelScene_CDC> _newlyAddedCards = new HashSet<DuelScene_CDC>();

	protected Vector3 _safeAreaOffset = Vector3.zero;

	public HashSet<DuelScene_CDC> SwapIgnoredCards = new HashSet<DuelScene_CDC>();

	protected List<CardLayoutData> _previousLayoutData = new List<CardLayoutData>();

	protected List<CardLayoutData> _secondaryLayoutData = new List<CardLayoutData>();

	private readonly HashSet<DuelScene_CDC> _laidOutCdcCache = new HashSet<DuelScene_CDC>();

	protected bool _visibility = true;

	public int Layer => base.gameObject.layer;

	public ICardLayout Layout { get; set; } = new CardLayout_Horizontal();

	public GREPlayerNum PlayerNum => playerNum;

	public CardHolderType CardHolderType => m_cardHolderType;

	public float CardScale => _cdcSize;

	public bool EnableShadows => _enableShadows;

	public List<DuelScene_CDC> CardViews => _cardViews;

	public virtual Transform EffectsRoot => base.transform;

	public virtual Transform CardRoot => base.transform;

	public bool IgnoreDummyCards { get; set; }

	public event System.Action OnCardHolderUpdated;

	public event Action<DuelScene_CDC> CardRemoved;

	public event Action<DuelScene_CDC> CardAdded;

	public virtual void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		_gameManager = gameManager;
		_assetLookupSystem = gameManager.AssetLookupSystem;
		_cardViewProvider = cardViewManager ?? NullCardViewProvider.Default;
		_vfxProvider = gameManager.VfxProvider ?? NullVfxProvider.Default;
		_locManager = locMan ?? NullLocProvider.Default;
		_splineMovementSystem = splineMovementSystem;
		_cardViewBuilder = cardViewBuilder;
		_matchManager = matchManager;
		if (UseSecondaryLayout)
		{
			SecondaryLayoutContainer.Init(gameManager, gameManager.GenericPool, this);
		}
		CardHoverController.OnHoveredCardUpdated += OnHoveredCardUpdated;
		OnSafeAreaChanged(ScreenEventController.Instance.GetSafeArea());
		ScreenEventController.Instance.OnSafeAreaChanged += OnSafeAreaChanged;
	}

	protected virtual void LateUpdate()
	{
		if (_isDirty)
		{
			_isDirty = false;
			OnPreLayout();
			LayoutNowInternal(_cardViews);
			_isUpdated = true;
		}
		else if (_isUpdated)
		{
			OnPostLayout();
			_isUpdated = false;
			this.OnCardHolderUpdated?.Invoke();
		}
	}

	protected virtual void OnDestroy()
	{
		CardHoverController.OnHoveredCardUpdated -= OnHoveredCardUpdated;
		if (ScreenEventController.Instance != null)
		{
			ScreenEventController.Instance.OnSafeAreaChanged -= OnSafeAreaChanged;
		}
		this.OnCardHolderUpdated = null;
		this.CardRemoved = null;
		this.CardAdded = null;
		_gameManager = null;
		_cardViewProvider = null;
		_splineMovementSystem = null;
		_cardViewBuilder = null;
		_cardViewProvider = NullCardViewProvider.Default;
		_vfxProvider = NullVfxProvider.Default;
		_locManager = NullLocProvider.Default;
	}

	protected virtual void OnHoveredCardUpdated(DuelScene_CDC hoveredCard)
	{
		if (UseSecondaryLayout && SecondaryLayoutContainer.UpdateSecondaryLayoutIdList(hoveredCard))
		{
			_isDirty = true;
		}
	}

	private void OnSafeAreaChanged(Rect newSafeArea)
	{
		Camera mainCamera = _gameManager.MainCamera;
		if ((object)mainCamera != null)
		{
			float num = Vector3.Distance(mainCamera.transform.position, base.transform.position);
			Vector3 vector = mainCamera.ScreenToWorldPoint(new Vector3(newSafeArea.xMin, (float)Screen.height / 2f, mainCamera.nearClipPlane + num));
			Vector3 vector2 = mainCamera.ScreenToWorldPoint(new Vector3(newSafeArea.xMax, (float)Screen.height / 2f, mainCamera.nearClipPlane + num));
			_safeAreaOffset.x = base.transform.InverseTransformPoint((vector + vector2) / 2f).x;
			_isDirty = true;
		}
	}

	public virtual void AddCard(DuelScene_CDC cardView)
	{
		HandleAddedCard(cardView);
		cardView.CurrentCardHolder = this;
		cardView.UpdateVisuals();
		cardView.Root.SetParent(CardRoot, worldPositionStays: true);
		cardView.Root.gameObject.SetLayer(base.gameObject.layer);
		_isDirty = true;
		this.CardAdded?.Invoke(cardView);
	}

	public virtual void SetCardAdded(DuelScene_CDC cardView)
	{
		_newlyAddedCards.Add(cardView);
	}

	protected virtual void HandleAddedCard(DuelScene_CDC cardView)
	{
		if (_cardViews.Contains(cardView))
		{
			return;
		}
		cardView.PreviousCardHolder = cardView.CurrentCardHolder;
		if (IgnoreDummyCards)
		{
			int num = -1;
			for (int i = 0; i < _cardViews.Count; i++)
			{
				if (_cardViews[i].Model.InstanceId == 0)
				{
					num = i;
					break;
				}
			}
			if (num > 0)
			{
				_cardViews.Insert(num, cardView);
			}
			else
			{
				_cardViews.Add(cardView);
			}
		}
		else
		{
			_cardViews.Add(cardView);
		}
		_newlyAddedCards.Add(cardView);
	}

	public virtual void RemoveCard(DuelScene_CDC cardView)
	{
		cardView.CollisionRoot.ZeroOut();
		if (_cardViews.Remove(cardView))
		{
			this.CardRemoved?.Invoke(cardView);
			_isDirty = true;
		}
	}

	public virtual void OnCardUpdated(DuelScene_CDC cardView)
	{
	}

	[ContextMenu("Force Layout")]
	public void LayoutNow()
	{
		_isDirty = true;
	}

	protected virtual void OnPreLayout()
	{
	}

	protected virtual void LayoutNowInternal(List<DuelScene_CDC> cardsToLayout, bool layoutInstantly = false)
	{
		_laidOutCdcCache.Clear();
		_previousLayoutData.Clear();
		_secondaryLayoutData.Clear();
		Layout.GenerateData(cardsToLayout, ref _previousLayoutData, GetLayoutCenterPoint(), GetLayoutRotation());
		if (UseSecondaryLayout)
		{
			SecondaryLayoutContainer.GenerateData(ref _previousLayoutData, ref _secondaryLayoutData);
		}
		for (int i = 0; i < _secondaryLayoutData.Count; i++)
		{
			CardLayoutData cardLayoutData = _secondaryLayoutData[i];
			if (_laidOutCdcCache.Add(cardLayoutData.Card))
			{
				cardLayoutData.Card.Root.parent = CardRoot;
				ApplyLayoutData(cardLayoutData, added: false, shouldBeVisible: true, layoutInstantly);
			}
		}
		for (int j = 0; j < _previousLayoutData.Count; j++)
		{
			CardLayoutData cardLayoutData2 = _previousLayoutData[j];
			if (_laidOutCdcCache.Add(cardLayoutData2.Card))
			{
				cardLayoutData2.Card.Root.parent = CardRoot;
				ApplyLayoutData(cardLayoutData2, _newlyAddedCards.Contains(cardLayoutData2.Card), CalcCardVisibility(cardLayoutData2, j), layoutInstantly);
			}
		}
		_laidOutCdcCache.Clear();
		_newlyAddedCards.Clear();
	}

	protected virtual Vector3 GetLayoutCenterPoint()
	{
		return Vector3.zero;
	}

	protected virtual Quaternion GetLayoutRotation()
	{
		return Quaternion.identity;
	}

	protected virtual void OnPostLayout()
	{
	}

	public virtual void SwapCards(int cardIndexOne, int cardIndexTwo)
	{
		DuelScene_CDC value = _cardViews[cardIndexOne];
		_cardViews[cardIndexOne] = _cardViews[cardIndexTwo];
		_cardViews[cardIndexTwo] = value;
		CardLayoutData cardLayoutData = _previousLayoutData.Find((CardLayoutData x) => x.Card == _cardViews[cardIndexOne]);
		CardLayoutData cardLayoutData2 = _previousLayoutData.Find((CardLayoutData x) => x.Card == _cardViews[cardIndexTwo]);
		if (cardLayoutData == null || cardLayoutData2 == null)
		{
			return;
		}
		cardLayoutData.Card = _cardViews[cardIndexTwo];
		cardLayoutData2.Card = _cardViews[cardIndexOne];
		IdealPoint layoutEndpoint = GetLayoutEndpoint(cardLayoutData);
		IdealPoint layoutEndpoint2 = GetLayoutEndpoint(cardLayoutData2);
		if (_splineMovementSystem != null)
		{
			if (cardLayoutData.Card != null)
			{
				_splineMovementSystem.AddPermanentGoal(cardLayoutData.Card.Root, layoutEndpoint);
			}
			if (cardLayoutData2.Card != null)
			{
				_splineMovementSystem.AddPermanentGoal(cardLayoutData2.Card.Root, layoutEndpoint2);
			}
		}
	}

	public virtual void ShiftCards(int cardIndex, int targetIndex)
	{
		while (cardIndex < targetIndex)
		{
			SwapCards(cardIndex, cardIndex + 1);
			cardIndex++;
		}
		while (cardIndex > targetIndex)
		{
			SwapCards(cardIndex, cardIndex - 1);
			cardIndex--;
		}
	}

	public virtual int GetIndexForCard(DuelScene_CDC cardView)
	{
		int count = _previousLayoutData.Count;
		for (int i = 0; i < count; i++)
		{
			if ((object)_previousLayoutData[i].Card == cardView)
			{
				return i;
			}
		}
		return -1;
	}

	public virtual int GetClosestCardIndexToPosition(float cardLocalX)
	{
		float num = float.MaxValue;
		int result = -1;
		for (int i = 0; i < _previousLayoutData.Count; i++)
		{
			CardLayoutData cardLayoutData = _previousLayoutData[i];
			DuelScene_CDC card = cardLayoutData.Card;
			if ((!IgnoreDummyCards || card.Model.InstanceId != 0) && !SwapIgnoredCards.Contains(card))
			{
				float num2 = Mathf.Abs(cardLayoutData.Position.x - cardLocalX);
				if (num2 < num)
				{
					num = num2;
					result = i;
				}
			}
		}
		return result;
	}

	protected virtual bool CalcCardVisibility(CardLayoutData data, int indexInList)
	{
		CardHolderType cardHolderType = CardHolderType;
		if ((uint)(cardHolderType - -1) <= 1u || cardHolderType == CardHolderType.OffCameraLibrary)
		{
			return false;
		}
		if (_visibility)
		{
			return data.IsVisibleInLayout;
		}
		return false;
	}

	public virtual void SetVisibility(bool visibility)
	{
		if (visibility != _visibility)
		{
			_visibility = visibility;
			LayoutNow();
		}
	}

	protected virtual void ApplyLayoutData(CardLayoutData data, bool added, bool shouldBeVisible, bool moveInstantly = false)
	{
		IdealPoint layoutEndpoint = GetLayoutEndpoint(data);
		if (moveInstantly)
		{
			MoveInstant(data, shouldBeVisible, layoutEndpoint);
		}
		else
		{
			MoveSpline(data, added, shouldBeVisible, layoutEndpoint);
		}
	}

	protected void MoveInstant(CardLayoutData data, bool shouldBeVisible, IdealPoint endpoint)
	{
		MoveCardInstant(data, endpoint);
		data.Card.UpdateVisibility(shouldBeVisible);
	}

	protected void MoveSpline(CardLayoutData data, bool added, bool shouldBeVisible, IdealPoint endpoint)
	{
		string text = (added ? GetLayoutSplinePath(data) : GetInternalLayoutSplinePath(data));
		SplineEventData splineEventData = new SplineEventData();
		if (!(data.Card.PreviousCardHolder is CardBrowserCardHolder))
		{
			splineEventData = ((!added) ? GetInternalLayoutSplineEvents(data) : GetLayoutSplineEvents(data));
		}
		if (shouldBeVisible || CardHolderType == CardHolderType.CardBrowserDefault || CardHolderType == CardHolderType.CardBrowserViewDismiss)
		{
			data.Card.UpdateVisibility(shouldBeVisible: true);
		}
		else if (!added && _splineMovementSystem.GetProgress(data.Card.Root) >= 1f)
		{
			data.Card.UpdateVisibility(shouldBeVisible: false);
		}
		else
		{
			DuelScene_CDC card = data.Card;
			if (card.IsVisible)
			{
				splineEventData.Events.Add(new SplineEventCallback(1f, delegate(float prog)
				{
					if (prog >= 1f && (CardHolderBase)card.CurrentCardHolder == this)
					{
						card.UpdateVisibility(shouldBeVisible: false);
					}
				}));
			}
		}
		if (added)
		{
			_splineMovementSystem.RemoveTemporaryGoal(data.Card.Root);
		}
		MoveSplineEvents(splineEventData, data);
		SplineMovementData spline = ((!string.IsNullOrEmpty(text)) ? _gameManager.SplineCache.Get(text) : null);
		_splineMovementSystem.AddPermanentGoal(data.Card.Root, endpoint, !added, spline, splineEventData);
	}

	protected virtual void MoveSplineEvents(SplineEventData events, CardLayoutData data)
	{
	}

	protected virtual void MoveCardInstant(CardLayoutData data, IdealPoint endpoint)
	{
		_splineMovementSystem.MoveInstant(data.Card.Root, endpoint);
	}

	public IdealPoint GetLayoutEndpoint(DuelScene_CDC cdc)
	{
		foreach (CardLayoutData previousLayoutDatum in _previousLayoutData)
		{
			if (previousLayoutDatum.Card == cdc)
			{
				return GetLayoutEndpoint(previousLayoutDatum);
			}
		}
		return new IdealPoint(cdc.transform);
	}

	public virtual IdealPoint GetLayoutEndpoint(CardLayoutData data)
	{
		Transform parent = data.Card.Root.parent;
		return new IdealPoint(parent.TransformPoint(data.Position), parent.rotation * data.Rotation, _overrideCdcSize ? (Vector3.one * _cdcSize) : data.Scale);
	}

	protected virtual string GetLayoutSplinePath(CardLayoutData data)
	{
		string result = null;
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(data.Card.Model);
		_assetLookupSystem.Blackboard.CardHolderType = CardHolderType;
		_assetLookupSystem.Blackboard.ZonePair = new ZonePair(data.Card.PreviousCardHolder, this);
		_assetLookupSystem.Blackboard.CardInsertionPosition = GetCardPosition(data.Card);
		if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<MovementPayload_Spline> loadedTree))
		{
			MovementPayload_Spline payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
			if (payload != null)
			{
				result = payload.SplineDataRef.RelativePath;
			}
		}
		_assetLookupSystem.Blackboard.Clear();
		return result;
	}

	protected virtual string GetInternalLayoutSplinePath(CardLayoutData data)
	{
		return null;
	}

	protected virtual SplineEventData GetLayoutSplineEvents(CardLayoutData data)
	{
		SplineEventData splineEvents = new SplineEventData();
		ZonePair fromToZoneForCard = CardViewUtilities.GetFromToZoneForCard(data.Card);
		if (fromToZoneForCard.ToZone == ZoneType.Stack || fromToZoneForCard.FromZone == ZoneType.Stack)
		{
			SetupSplineEventsALT<MovementPayload_Stack_VFX, MovementPayload_Stack_SFX>(data, added: true, ref splineEvents, GetCardPosition(data.Card), fromToZoneForCard, _gameManager);
		}
		else
		{
			SetupSplineEventsALT<MovementPayload_NonStack_VFX, MovementPayload_NonStack_SFX>(data, added: true, ref splineEvents, GetCardPosition(data.Card), fromToZoneForCard, _gameManager);
		}
		return splineEvents;
	}

	protected virtual SplineEventData GetInternalLayoutSplineEvents(CardLayoutData data)
	{
		return new SplineEventData();
	}

	protected CardPosition GetCardPosition(DuelScene_CDC card)
	{
		if (_cardViews.Count == 0)
		{
			return CardPosition.None;
		}
		if (_cardViews[0] == card)
		{
			return CardPosition.Top;
		}
		if (_cardViews[_cardViews.Count - 1] == card)
		{
			return CardPosition.Bottom;
		}
		return CardPosition.Middle;
	}

	public static void SetupSplineEventsALT<T, U>(CardLayoutData data, bool added, ref SplineEventData splineEvents, CardPosition cardPosition, ZonePair zonePair, GameManager gameManager) where T : MovementVFX where U : MovementSFX
	{
		IObjectPool genericPool = gameManager.GenericPool;
		AssetLookupSystem assetLookupSystem = gameManager.AssetLookupSystem;
		if (!data.IsVisibleInLayout && !added && cardPosition != CardPosition.Top)
		{
			return;
		}
		DuelScene_CDC card = data.Card;
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(card.Model);
		assetLookupSystem.Blackboard.CardHolderType = data.Card.CurrentCardHolder.CardHolderType;
		assetLookupSystem.Blackboard.ZonePair = zonePair;
		assetLookupSystem.Blackboard.CardInsertionPosition = cardPosition;
		assetLookupSystem.Blackboard.Player = card.Model.Controller;
		assetLookupSystem.Blackboard.IdealWorldPosition = data.Position;
		AssetLookupTree<T> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<T>();
		AssetLookupTree<U> assetLookupTree2 = assetLookupSystem.TreeLoader.LoadTree<U>();
		HashSet<T> hashSet = genericPool.PopObject<HashSet<T>>();
		HashSet<U> hashSet2 = genericPool.PopObject<HashSet<U>>();
		assetLookupTree.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet);
		assetLookupTree2.GetPayloadLayered(assetLookupSystem.Blackboard, hashSet2);
		if (hashSet.Count > 0)
		{
			foreach (T item in hashSet)
			{
				U val = hashSet2.Find(item, (U x, T y) => x.Layers.SetEquals(y.Layers));
				foreach (VfxData vfxData in item.VfxDatas)
				{
					if (vfxData.PrefabData == null)
					{
						continue;
					}
					float time = Mathf.Clamp01(vfxData.PrefabData.StartTime);
					if (val != null)
					{
						splineEvents.Events.Add(new SplineEventAudio(time, val.SfxData.AudioEvents, card.Root.gameObject));
						hashSet2.Remove(val);
						val = null;
					}
					splineEvents.Events.Add(new SplineEventCallbackWithParams<(IVfxProvider, DuelScene_CDC, VfxData)>(time, (gameManager.VfxProvider, data.Card, vfxData), delegate(float _, (IVfxProvider vfxProvider, DuelScene_CDC card, VfxData vfxData) paramBlob)
					{
						if (!(paramBlob.card == null) && !(paramBlob.card.EffectsRoot == null))
						{
							paramBlob.vfxProvider.PlayVFX(paramBlob.vfxData, paramBlob.card.Model);
						}
					}));
				}
			}
			if (hashSet2.Count > 0)
			{
				foreach (U item2 in hashSet2)
				{
					splineEvents.Events.Add(new SplineEventAudio(0f, item2.SfxData.AudioEvents, card.Root.gameObject));
				}
			}
		}
		else if (hashSet2.Count > 0)
		{
			foreach (U item3 in hashSet2)
			{
				splineEvents.Events.Add(new SplineEventAudio(0f, item3.SfxData.AudioEvents, card.Root.gameObject));
			}
		}
		hashSet.Clear();
		hashSet2.Clear();
		genericPool.PushObject(hashSet, tryClear: false);
		genericPool.PushObject(hashSet2, tryClear: false);
	}

	protected void ClampHoveredCardCollision()
	{
		DuelScene_CDC draggedCard = CardDragController.DraggedCard;
		foreach (CardLayoutData previousLayoutDatum in _previousLayoutData)
		{
			Transform collisionRoot = previousLayoutDatum.Card.CollisionRoot;
			if (previousLayoutDatum.Card == draggedCard)
			{
				collisionRoot.ZeroOut();
				continue;
			}
			Vector3 localScale = previousLayoutDatum.Card.Root.localScale;
			IdealPoint layoutEndpoint = GetLayoutEndpoint(previousLayoutDatum);
			collisionRoot.position = layoutEndpoint.Position;
			collisionRoot.rotation = layoutEndpoint.Rotation;
			collisionRoot.localScale = new Vector3((localScale.x > 0f) ? (previousLayoutDatum.Scale.x / localScale.x) : 0f, (localScale.y > 0f) ? (previousLayoutDatum.Scale.y / localScale.y) : 0f, (localScale.z > 0f) ? (previousLayoutDatum.Scale.z / localScale.z) : 0f);
		}
	}

	protected virtual void OnDrawGizmos()
	{
		if (UseSecondaryLayout)
		{
			SecondaryLayoutContainer.DrawGizmos(_previousLayoutData);
		}
	}
}
