using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Shared.Code.CardFilters;
using DG.Tweening;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wizards.GeneralUtilities.Extensions;
using Wotc.Mtga;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

public class StaticColumnManager : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	private bool _hasBackingDropTargetInitialized;

	[SerializeField]
	private StaticColumnDropTarget[] _backingDropTargets;

	[SerializeField]
	private RectTransform _cardColumnParent;

	[SerializeField]
	private StaticColumnDropTarget _newColumnDropTargetPrefab;

	[SerializeField]
	private RectTransform _newColumnDropTargetParent;

	[SerializeField]
	private StaticColumnMetaCardHolder _holderPrefab;

	[SerializeField]
	private RectTransform _holderParent;

	[SerializeField]
	private RectTransform _quantityAdjustParent;

	[Header("Card Layout")]
	public float CardHeight = 5.6f;

	[Tooltip("Rotation so cards do not z fight on plane. \n Should be countered exactly in viewport rotation.")]
	public Vector3 CardStackRotation = new Vector3(-8f, 0f, 0f);

	public Vector3 StartPos = new Vector3(-13.8f, 0f, 0f);

	public Vector3 CardInstantiationOffset = new Vector3(0f, 0f, -9f);

	public float ColumnSpacing = 4.6f;

	[Tooltip("Snap spacing, requires scroll rect callback")]
	public Vector3 SnapSpacing = Vector3.zero;

	public Vector3 TightSnapSpacing = Vector3.zero;

	public bool ExpandedView;

	public float RepositionMoveDuration = 0.2f;

	public StaticColumnPool CardPool;

	public ScrollRect scrollRect;

	public Vector2 padding;

	public RectTransform contentBounds;

	public float SideDropTargetScale = 50f;

	public float InnerDropTargetOffset = -2.55f;

	public Func<uint, bool> CanAddCard;

	public CommanderSlotCardHolder CommanderCardHolder;

	public CommanderSlotCardHolder PartnerCardHolder;

	public CommanderSlotCardHolder CompanionCardHolder;

	protected List<StaticColumnMetaCardHolder> _allColumns = new List<StaticColumnMetaCardHolder>();

	protected List<StaticColumnDropTarget> _allNewColumnDropTargets = new List<StaticColumnDropTarget>();

	private uint _droppingGrpId;

	private float _droppingIntoColumnNumber;

	private Vector3 _droppingAtPosition;

	private const float NEAR_ZERO = 1E-05f;

	private const float NEAR_ONE = 0.99999f;

	protected CardDatabase _cardDatabase;

	protected CardViewBuilder _cardViewBuilder;

	protected AssetLookupSystem _assetLookupSystem;

	[SerializeField]
	private bool _sendDragEventsUp;

	private Func<MetaCardView, bool> _canDragCards;

	private Func<MetaCardView, bool> _canSingleClickCards;

	private Func<MetaCardView, bool> _canDoubleClickCards;

	private Func<MetaCardView, bool> _canDropCards;

	private Action<MetaCardView, bool, bool, bool> _customHighlightHandler;

	private ICardRolloverZoom _rolloverZoomView;

	private const int MIN_COLUMN_CMC = 1;

	private const int MAX_COLUMN_CMC = 6;

	public ColumnCardQuantityAdjust QuantityAdjust { get; private set; }

	public Action<MetaCardView, MetaCardHolder> OnCardDropped { get; set; }

	public Action<MetaCardView> OnCardClicked { get; set; }

	public Action<MetaCardView> OnCardUpdated { get; set; }

	public bool SendDragEventsUp
	{
		get
		{
			return _sendDragEventsUp;
		}
		set
		{
			_sendDragEventsUp = value;
			foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
			{
				allColumn.SendDragEventsUp = _sendDragEventsUp;
			}
		}
	}

	public Func<MetaCardView, bool> CanDragCards
	{
		get
		{
			return _canDragCards;
		}
		set
		{
			_canDragCards = value;
			InitBackingDropTargets();
			StaticColumnDropTarget[] backingDropTargets = _backingDropTargets;
			for (int i = 0; i < backingDropTargets.Length; i++)
			{
				backingDropTargets[i].CanDragCards = _canDragCards;
			}
			foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
			{
				allColumn.CanDragCards = _canDragCards;
			}
		}
	}

	public Func<MetaCardView, bool> CanSingleClickCards
	{
		get
		{
			return _canSingleClickCards;
		}
		set
		{
			_canSingleClickCards = value;
			InitBackingDropTargets();
			StaticColumnDropTarget[] backingDropTargets = _backingDropTargets;
			for (int i = 0; i < backingDropTargets.Length; i++)
			{
				backingDropTargets[i].CanSingleClickCards = _canSingleClickCards;
			}
			foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
			{
				allColumn.CanSingleClickCards = _canSingleClickCards;
			}
		}
	}

	public Func<MetaCardView, bool> CanDoubleClickCards
	{
		get
		{
			return _canDoubleClickCards;
		}
		set
		{
			_canDoubleClickCards = value;
			InitBackingDropTargets();
			StaticColumnDropTarget[] backingDropTargets = _backingDropTargets;
			for (int i = 0; i < backingDropTargets.Length; i++)
			{
				backingDropTargets[i].CanDoubleClickCards = _canDoubleClickCards;
			}
			foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
			{
				allColumn.CanDoubleClickCards = _canDoubleClickCards;
			}
		}
	}

	public Func<MetaCardView, bool> CanDropCards
	{
		get
		{
			return _canDragCards;
		}
		set
		{
			_canDropCards = value;
			InitBackingDropTargets();
			StaticColumnDropTarget[] backingDropTargets = _backingDropTargets;
			for (int i = 0; i < backingDropTargets.Length; i++)
			{
				backingDropTargets[i].CanDropCards = _canDropCards;
			}
			foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
			{
				allColumn.CanDropCards = _canDropCards;
			}
		}
	}

	public Action<MetaCardView, bool, bool, bool> CustomHighlightHandler
	{
		get
		{
			return _customHighlightHandler;
		}
		set
		{
			_customHighlightHandler = value;
			InitBackingDropTargets();
			StaticColumnDropTarget[] backingDropTargets = _backingDropTargets;
			for (int i = 0; i < backingDropTargets.Length; i++)
			{
				backingDropTargets[i].CustomHighlightHandler = _customHighlightHandler;
			}
			foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
			{
				allColumn.CustomHighlightHandler = _customHighlightHandler;
			}
		}
	}

	public ICardRolloverZoom RolloverZoomView
	{
		get
		{
			return _rolloverZoomView;
		}
		set
		{
			_rolloverZoomView = value;
			foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
			{
				allColumn.RolloverZoomView = value;
			}
		}
	}

	public void Init(AssetLookupSystem assetLookupSystem, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder)
	{
		_cardDatabase = cardDatabase;
		_cardViewBuilder = cardViewBuilder;
		_assetLookupSystem = assetLookupSystem;
		string prefabPath = assetLookupSystem.GetPrefabPath<CardQuantityAdjustPrefab, ColumnCardQuantityAdjust>();
		QuantityAdjust = AssetLoader.Instantiate<ColumnCardQuantityAdjust>(prefabPath, _quantityAdjustParent);
		ColumnCardQuantityAdjust quantityAdjust = QuantityAdjust;
		quantityAdjust.OnTimeout = (System.Action)Delegate.Combine(quantityAdjust.OnTimeout, new System.Action(OnQuantityAdjustTimeout));
	}

	private void OnDestroy()
	{
		ColumnCardQuantityAdjust quantityAdjust = QuantityAdjust;
		quantityAdjust.OnTimeout = (System.Action)Delegate.Remove(quantityAdjust.OnTimeout, new System.Action(OnQuantityAdjustTimeout));
	}

	private void OnQuantityAdjustTimeout()
	{
		OnHideQuantityAdjust(repositionCards: true);
	}

	public void OnBackgroundClick()
	{
		OnHideQuantityAdjust(repositionCards: true);
	}

	public void ClearCommanderCards()
	{
		CommanderCardHolder.ClearCards();
		CommanderCardHolder.gameObject.SetActive(value: false);
		PartnerCardHolder.gameObject.SetActive(value: false);
	}

	public void SetCommanderCards(ListMetaCardViewDisplayInformation di, bool ignoreOwnership)
	{
		CommanderCardHolder.gameObject.SetActive(value: true);
		if (di.Card != null)
		{
			CommanderCardHolder.SetCard(di, ignoreOwnership);
			return;
		}
		CommanderCardHolder.ClearCards();
		PartnerCardHolder.gameObject.SetActive(value: false);
	}

	public void SetCompanionCard(ListMetaCardViewDisplayInformation di, bool ignoreOwnership)
	{
		CompanionCardHolder.gameObject.SetActive(value: true);
		if (di.Card != null)
		{
			CompanionCardHolder.SetCard(di, ignoreOwnership);
		}
		else
		{
			CompanionCardHolder.ClearCards();
		}
	}

	public void ClearCompanionCard()
	{
		CompanionCardHolder.ClearCards();
		CompanionCardHolder.gameObject.SetActive(value: false);
	}

	public void ClearCards()
	{
		foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
		{
			ReleaseColumn(allColumn);
		}
		_allColumns.Clear();
	}

	public void SetCards(List<ListMetaCardViewDisplayInformation> allCards, CardFilter textSearchFilter = null)
	{
		Dictionary<float, List<ListMetaCardViewDisplayInformation>> dictionary = new Dictionary<float, List<ListMetaCardViewDisplayInformation>>();
		foreach (ListMetaCardViewDisplayInformation item in allCards)
		{
			if (item.Quantity == 0)
			{
				continue;
			}
			float key;
			if (_droppingGrpId == item.Card.GrpId)
			{
				key = _droppingIntoColumnNumber;
			}
			else
			{
				StaticColumnMetaCardHolder staticColumnMetaCardHolder = _allColumns.FirstOrDefault((StaticColumnMetaCardHolder c) => c.ContainsCard(item.Card.GrpId));
				key = ((staticColumnMetaCardHolder == null) ? ((float)GetNaturalColumnNumber(item.Card)) : staticColumnMetaCardHolder.ColumnNumber);
			}
			dictionary.GetOrCreate(key).Add(item);
		}
		List<StaticColumnMetaCardHolder> list = new List<StaticColumnMetaCardHolder>(_allColumns);
		_allColumns.Clear();
		DestroyAllNewColumnDropTargets();
		Vector3 startPos = StartPos;
		DOTween.Kill(this);
		float num = 0f;
		uint? grpQuantityAdjust = QuantityAdjust.CurrentCardData?.GrpId;
		foreach (KeyValuePair<float, List<ListMetaCardViewDisplayInformation>> item2 in dictionary.OrderBy((KeyValuePair<float, List<ListMetaCardViewDisplayInformation>> pair) => pair.Key))
		{
			item2.Deconstruct(out var key2, out var value);
			float columnNumber = key2;
			List<ListMetaCardViewDisplayInformation> list2 = value;
			StaticColumnMetaCardHolder staticColumnMetaCardHolder2 = list.FirstOrDefault((StaticColumnMetaCardHolder c) => c.ColumnNumber == columnNumber);
			if (staticColumnMetaCardHolder2 == null)
			{
				staticColumnMetaCardHolder2 = UnityEngine.Object.Instantiate(_holderPrefab);
				Transform obj = staticColumnMetaCardHolder2.transform;
				obj.SetParent(_holderParent);
				obj.ZeroOut();
				obj.localPosition = startPos;
				staticColumnMetaCardHolder2.EnsureInit(_cardDatabase, _cardViewBuilder);
				staticColumnMetaCardHolder2.CanDragCards = _canDragCards;
				staticColumnMetaCardHolder2.CanDropCards = _canDropCards;
				staticColumnMetaCardHolder2.CanDoubleClickCards = _canDoubleClickCards;
				staticColumnMetaCardHolder2.CanSingleClickCards = _canSingleClickCards;
				staticColumnMetaCardHolder2.RolloverZoomView = _rolloverZoomView;
				staticColumnMetaCardHolder2.ColumnNumber = columnNumber;
				staticColumnMetaCardHolder2.CardStackRotation = CardStackRotation;
				staticColumnMetaCardHolder2.CardInstantiationOffset = CardInstantiationOffset;
				staticColumnMetaCardHolder2.CardHeight = CardHeight;
				staticColumnMetaCardHolder2.RepositionMoveDuration = RepositionMoveDuration;
				staticColumnMetaCardHolder2.CardPool = CardPool;
				StaticColumnMetaCardHolder staticColumnMetaCardHolder3 = staticColumnMetaCardHolder2;
				staticColumnMetaCardHolder3.OnCardClicked = (Action<MetaCardView>)Delegate.Combine(staticColumnMetaCardHolder3.OnCardClicked, new Action<MetaCardView>(Column_OnCardClicked));
				StaticColumnMetaCardHolder staticColumnMetaCardHolder4 = staticColumnMetaCardHolder2;
				staticColumnMetaCardHolder4.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(staticColumnMetaCardHolder4.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(SubHolder_OnCardDropped));
				StaticColumnMetaCardHolder staticColumnMetaCardHolder5 = staticColumnMetaCardHolder2;
				staticColumnMetaCardHolder5.OnCardDragged = (Action<MetaCardView>)Delegate.Combine(staticColumnMetaCardHolder5.OnCardDragged, new Action<MetaCardView>(OnDragCard));
				staticColumnMetaCardHolder2.SendDragEventsUp = SendDragEventsUp;
				UpdateCardSpacingValues(staticColumnMetaCardHolder2);
			}
			else
			{
				list.Remove(staticColumnMetaCardHolder2);
			}
			_allColumns.Add(staticColumnMetaCardHolder2);
			if (staticColumnMetaCardHolder2.ColumnNumber == _droppingIntoColumnNumber)
			{
				staticColumnMetaCardHolder2.RegisterPosition(_droppingGrpId, _droppingAtPosition);
			}
			staticColumnMetaCardHolder2.SetCards(_assetLookupSystem, list2, textSearchFilter, grpQuantityAdjust);
			foreach (StaticColumnMetaCardView cardView in staticColumnMetaCardHolder2.CardViews)
			{
				OnCardUpdated?.Invoke(cardView);
			}
			if (staticColumnMetaCardHolder2.CardViews.All((StaticColumnMetaCardView card) => card.RecentlyCreated))
			{
				staticColumnMetaCardHolder2.transform.localPosition = startPos;
			}
			else if (staticColumnMetaCardHolder2.transform.localPosition != startPos)
			{
				staticColumnMetaCardHolder2.transform.DOLocalMove(startPos, RepositionMoveDuration).SetEase(Ease.InOutSine).SetTarget(this);
			}
			startPos.x += ColumnSpacing;
			if (num < staticColumnMetaCardHolder2.UpperColumnHeight)
			{
				num = staticColumnMetaCardHolder2.UpperColumnHeight;
			}
		}
		foreach (StaticColumnMetaCardHolder item3 in list)
		{
			ReleaseColumn(item3);
		}
		Vector3 startPos2 = StartPos;
		startPos2.x -= 0.5f * ColumnSpacing;
		for (int num2 = 0; num2 < _allColumns.Count; num2++)
		{
			StaticColumnMetaCardHolder staticColumnMetaCardHolder6 = _allColumns[num2];
			staticColumnMetaCardHolder6.UpperColumnHeight = num;
			UpdateCardSpacingValues(staticColumnMetaCardHolder6);
			staticColumnMetaCardHolder6.RepositionAllCards(_assetLookupSystem, grpQuantityAdjust);
			float num3 = Mathf.Ceil(staticColumnMetaCardHolder6.ColumnNumber - 1f);
			if (num2 == 0)
			{
				StaticColumnDropTarget target = CreateNewColumnDropTarget(0.5f * (num3 + staticColumnMetaCardHolder6.ColumnNumber), startPos2);
				UpdateSideDropTargetScale(target, -1f);
			}
			else
			{
				num3 = Mathf.Max(num3, _allColumns[num2 - 1].ColumnNumber);
				CreateNewColumnDropTarget(0.5f * (num3 + staticColumnMetaCardHolder6.ColumnNumber), startPos2).CanDropOffset = InnerDropTargetOffset;
			}
			startPos2.x += ColumnSpacing;
			if (num2 == _allColumns.Count - 1)
			{
				float num4 = Mathf.Floor(staticColumnMetaCardHolder6.ColumnNumber + 1f);
				StaticColumnDropTarget target2 = CreateNewColumnDropTarget(0.5f * (num4 + staticColumnMetaCardHolder6.ColumnNumber), startPos2);
				UpdateSideDropTargetScale(target2, 1f);
			}
		}
		QuantityAdjust.Refresh();
		UpdateContentSize();
		_droppingGrpId = 0u;
		_droppingIntoColumnNumber = -1f;
		_droppingAtPosition = Vector3.zero;
	}

	public void ShowQuantityAdjust(StaticColumnMetaCardView cardView, bool repositionCards)
	{
		RolloverZoomView.Close();
		QuantityAdjust.Show(cardView);
		if (!repositionCards)
		{
			return;
		}
		foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
		{
			allColumn.RepositionAllCards(_assetLookupSystem, cardView.Card.GrpId);
		}
	}

	public void OnHideQuantityAdjust(bool repositionCards)
	{
		QuantityAdjust.Hide();
		if (!repositionCards)
		{
			return;
		}
		foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
		{
			allColumn.RepositionAllCards(_assetLookupSystem, null);
		}
	}

	public void UpdateCardSpacingValues(StaticColumnMetaCardHolder column)
	{
		column.IsExpanded = ExpandedView;
	}

	private void UpdateSideDropTargetScale(StaticColumnDropTarget target, float posMult)
	{
		if (!(SideDropTargetScale <= 0f))
		{
			Transform colliderTransform = target.ColliderTransform;
			Vector3 localPosition = colliderTransform.localPosition;
			Vector3 localScale = colliderTransform.localScale;
			localPosition.x += posMult * 0.5f * (SideDropTargetScale - localScale.x);
			colliderTransform.localPosition = localPosition;
			localScale.x = SideDropTargetScale;
			colliderTransform.localScale = localScale;
		}
	}

	private void UpdateContentSize()
	{
		if ((bool)contentBounds)
		{
			float num = ((_allColumns.Count == 0) ? 0f : ((_allColumns.Max((StaticColumnMetaCardHolder c) => c.TotalHeight) - StartPos.y - CardHeight / 2f) * _holderParent.localScale.y));
			float num2 = ColumnSpacing * (float)_allColumns.Count * _holderParent.localScale.x;
			contentBounds.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, num2 + padding.x);
			contentBounds.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, num + padding.y);
		}
	}

	public void UpdateDepthByScroll(Vector2 position)
	{
		if ((bool)contentBounds)
		{
			Vector3 localPosition = contentBounds.localPosition;
			localPosition.z = 0f;
			contentBounds.localPosition = localPosition;
		}
	}

	public void UpdateDepthWithSnapping(Vector2 position)
	{
		Vector3 localPosition = contentBounds.localPosition;
		Vector3 vector = Vector3.Scale(_holderParent.localScale, ExpandedView ? SnapSpacing : TightSnapSpacing);
		if (1E-05f < scrollRect.horizontalNormalizedPosition && scrollRect.horizontalNormalizedPosition < 0.99999f)
		{
			localPosition.x = Mathf.Floor(localPosition.x / vector.x) * vector.x;
		}
		else
		{
			scrollRect.horizontalNormalizedPosition = Mathf.Round(scrollRect.horizontalNormalizedPosition);
			scrollRect.velocity = Vector3.Scale(scrollRect.velocity, new Vector3(0f, 1f, 1f));
		}
		if (1E-05f < scrollRect.verticalNormalizedPosition && scrollRect.verticalNormalizedPosition < 0.99999f)
		{
			localPosition.y = Mathf.Floor(localPosition.y / vector.y) * vector.y;
		}
		else
		{
			scrollRect.verticalNormalizedPosition = Mathf.Round(scrollRect.verticalNormalizedPosition);
			scrollRect.velocity = Vector3.Scale(scrollRect.velocity, new Vector3(1f, 0f, 1f));
		}
		localPosition.z = 0f;
		contentBounds.localPosition = localPosition;
	}

	public void ScrollToTop()
	{
		scrollRect.normalizedPosition = new Vector2(0f, 1f);
		scrollRect.velocity = Vector2.zero;
	}

	public bool ContainsHolder(MetaCardHolder holder)
	{
		if (((IReadOnlyCollection<MetaCardHolder>)(object)_backingDropTargets).Contains(holder))
		{
			return true;
		}
		foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
		{
			if (holder == allColumn)
			{
				return true;
			}
		}
		foreach (StaticColumnDropTarget allNewColumnDropTarget in _allNewColumnDropTargets)
		{
			if (holder == allNewColumnDropTarget)
			{
				return true;
			}
		}
		return false;
	}

	private void Start()
	{
		ScrollToTop();
		InitBackingDropTargets();
		StaticColumnDropTarget[] backingDropTargets = _backingDropTargets;
		foreach (StaticColumnDropTarget obj in backingDropTargets)
		{
			obj.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(obj.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(SubHolder_OnCardDropped));
		}
	}

	private void SubHolder_OnCardDropped(MetaCardView cardView, MetaCardHolder destinationHolder)
	{
		if (!(cardView.Holder is CardPoolHolder) || CanAddCard == null || CanAddCard(cardView.Card.GrpId))
		{
			CDCMetaCardView cDCMetaCardView = cardView as CDCMetaCardView;
			_droppingAtPosition = ((cDCMetaCardView == null) ? new Vector3(0f, 0f, -9f) : cDCMetaCardView.CardView.Root.position);
			StaticColumnMetaCardHolder staticColumnMetaCardHolder = destinationHolder as StaticColumnMetaCardHolder;
			StaticColumnDropTarget staticColumnDropTarget = destinationHolder as StaticColumnDropTarget;
			if (staticColumnMetaCardHolder != null)
			{
				_droppingIntoColumnNumber = staticColumnMetaCardHolder.ColumnNumber;
			}
			else if (staticColumnDropTarget != null)
			{
				_droppingIntoColumnNumber = (staticColumnDropTarget.NaturalColumnNumber ? ((float)GetNaturalColumnNumber(cardView.Card.Printing)) : staticColumnDropTarget.SpecificColumnNumber);
			}
			else
			{
				_droppingIntoColumnNumber = GetNaturalColumnNumber(cardView.Card.Printing);
			}
			_droppingGrpId = cardView.Card.GrpId;
			OnCardDropped(cardView, destinationHolder);
		}
	}

	private void Column_OnCardClicked(MetaCardView cardView)
	{
		OnCardClicked(cardView);
	}

	private void OnDragCard(MetaCardView cardView)
	{
		OnHideQuantityAdjust(repositionCards: true);
	}

	private void ReleaseColumn(StaticColumnMetaCardHolder column)
	{
		column.ClearCards();
		column.OnCardClicked = (Action<MetaCardView>)Delegate.Remove(column.OnCardClicked, new Action<MetaCardView>(Column_OnCardClicked));
		column.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(column.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(SubHolder_OnCardDropped));
		column.OnCardDragged = (Action<MetaCardView>)Delegate.Remove(column.OnCardDragged, new Action<MetaCardView>(OnDragCard));
		UnityEngine.Object.Destroy(column.gameObject);
	}

	public void ReleaseDraggedCards()
	{
		foreach (StaticColumnMetaCardHolder allColumn in _allColumns)
		{
			allColumn.ReleaseAllDraggingCards();
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
	}

	public void OnPointerExit(PointerEventData eventData)
	{
	}

	private StaticColumnDropTarget CreateNewColumnDropTarget(float columnNumber, Vector3 localPosition)
	{
		StaticColumnDropTarget staticColumnDropTarget = UnityEngine.Object.Instantiate(_newColumnDropTargetPrefab);
		staticColumnDropTarget.EnsureInit(_cardDatabase, _cardViewBuilder);
		staticColumnDropTarget.CanDragCards = _canDragCards;
		staticColumnDropTarget.CanDropCards = _canDropCards;
		_allNewColumnDropTargets.Add(staticColumnDropTarget);
		staticColumnDropTarget.transform.SetParent(_newColumnDropTargetParent);
		staticColumnDropTarget.transform.localScale = Vector3.one;
		staticColumnDropTarget.transform.localRotation = Quaternion.identity;
		staticColumnDropTarget.transform.localPosition = localPosition;
		staticColumnDropTarget.SpecificColumnNumber = columnNumber;
		staticColumnDropTarget.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Combine(staticColumnDropTarget.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(SubHolder_OnCardDropped));
		return staticColumnDropTarget;
	}

	private void DestroyAllNewColumnDropTargets()
	{
		foreach (StaticColumnDropTarget allNewColumnDropTarget in _allNewColumnDropTargets)
		{
			allNewColumnDropTarget.OnCardDropped = (Action<MetaCardView, MetaCardHolder>)Delegate.Remove(allNewColumnDropTarget.OnCardDropped, new Action<MetaCardView, MetaCardHolder>(SubHolder_OnCardDropped));
		}
		_newColumnDropTargetParent.DestroyChildren();
		_allNewColumnDropTargets.Clear();
	}

	private static int GetNaturalColumnNumber(CardPrintingData cardData)
	{
		if (!cardData.Types.Contains(CardType.Land))
		{
			return Math.Clamp((int)cardData.ConvertedManaCost, 1, 6);
		}
		return 7;
	}

	private void InitBackingDropTargets()
	{
		if (!_hasBackingDropTargetInitialized)
		{
			_hasBackingDropTargetInitialized = true;
			StaticColumnDropTarget[] backingDropTargets = _backingDropTargets;
			for (int i = 0; i < backingDropTargets.Length; i++)
			{
				backingDropTargets[i].EnsureInit(_cardDatabase, _cardViewBuilder);
			}
		}
	}
}
