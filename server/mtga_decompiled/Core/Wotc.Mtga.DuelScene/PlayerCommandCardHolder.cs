using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.Card;
using AssetLookupTree.Payloads.MiniCDC;
using GreClient.CardData;
using GreClient.Rules;
using MovementSystem;
using Pooling;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.DuelScene.Command;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class PlayerCommandCardHolder : ZoneCardHolderBase, IGameEffectController
{
	private class CardLayout_Command : ICardLayout
	{
		private GameManager _gameManager;

		private AssetLookupSystem _assetLookupSystem;

		private PlayerCommandCardHolder _commandHolder;

		private RegionPagingButton _upPagingButton;

		private RegionPagingButton _downPagingButton;

		private int _currentOffset;

		public List<CardStack> CardStacks { get; private set; } = new List<CardStack>();

		private bool OpponentHolder => _commandHolder.PlayerNum != GREPlayerNum.LocalPlayer;

		public int PagedOutUp => _currentOffset;

		public int PagedOutDown => Mathf.Max(CardStacks.Count - _currentOffset - _commandHolder.MaxCardsShown, 0);

		public bool ShowPagingButtons { get; set; }

		public CardLayout_Command(PlayerCommandCardHolder commandHolder, RegionPagingButton upButtonPrefab, RegionPagingButton downButtonPrefab, GameManager gameManager, AssetLookupSystem assetLookupSystem)
		{
			_gameManager = gameManager;
			_assetLookupSystem = assetLookupSystem;
			_commandHolder = commandHolder;
			IUnityObjectPool unityObjectPool = Pantry.Get<IUnityObjectPool>();
			_upPagingButton = unityObjectPool.PopObject(upButtonPrefab.gameObject).GetComponent<RegionPagingButton>();
			_downPagingButton = unityObjectPool.PopObject(downButtonPrefab.gameObject).GetComponent<RegionPagingButton>();
			_upPagingButton.transform.SetParent(_commandHolder.transform);
			_downPagingButton.transform.SetParent(_commandHolder.transform);
			_upPagingButton.transform.ZeroOut();
			_downPagingButton.transform.ZeroOut();
			_upPagingButton.transform.localScale = _commandHolder.PagingButtonSize;
			_downPagingButton.transform.localScale = _commandHolder.PagingButtonSize;
			float num = _commandHolder.SpaceBetweenCards * (float)(_commandHolder.MaxCardsShown - 1) + commandHolder.StartCardYPosition;
			_upPagingButton.transform.localPosition = (OpponentHolder ? (Vector3.up * num + _commandHolder._pageButtonOffset) : (Vector3.down * _commandHolder.StartCardYPosition + _commandHolder._pageButtonOffset));
			_downPagingButton.transform.localPosition = (OpponentHolder ? (Vector3.up * _commandHolder.StartCardYPosition + Vector3.Scale(new Vector3(1f, -1f, 1f), _commandHolder._pageButtonOffset)) : (Vector3.down * num + Vector3.Scale(new Vector3(1f, -1f, 1f), _commandHolder._pageButtonOffset)));
			_upPagingButton.gameObject.SetActive(value: false);
			_downPagingButton.gameObject.SetActive(value: false);
			_upPagingButton.SetCount(0);
			_downPagingButton.SetCount(0);
			_upPagingButton.SetCallback(PageUp);
			_downPagingButton.SetCallback(PageDown);
		}

		public void GenerateData(List<DuelScene_CDC> allCardViews, ref List<CardLayoutData> allData, Vector3 center, Quaternion rotation)
		{
			allCardViews.Sort(CardViewComparator);
			CardStacks.Clear();
			for (int i = 0; i < allCardViews.Count; i++)
			{
				DuelScene_CDC duelScene_CDC = allCardViews[i];
				CardStack cardStack = CardStacks.Find(_assetLookupSystem, duelScene_CDC.Model, (CardStack t, AssetLookupSystem u, ICardDataAdapter v) => t.CanStack(u, v));
				if (cardStack == null)
				{
					CardStacks.Add(new CardStack(duelScene_CDC, _gameManager));
				}
				else
				{
					cardStack.AddCardToStack(duelScene_CDC);
				}
			}
			if (CardStacks.Count > _commandHolder.MaxCardsShown)
			{
				_upPagingButton.SetCount(PagedOutUp);
				_downPagingButton.SetCount(PagedOutDown);
				_upPagingButton.gameObject.SetActive(ShowPagingButtons);
				_downPagingButton.gameObject.SetActive(ShowPagingButtons);
			}
			else
			{
				_currentOffset = 0;
				_upPagingButton.gameObject.SetActive(value: false);
				_downPagingButton.gameObject.SetActive(value: false);
			}
			int currentOffset = _currentOffset;
			int num = Mathf.Min(_currentOffset + _commandHolder.MaxCardsShown, CardStacks.Count);
			Vector3 vector = Vector3.left * _commandHolder.OffscreenXOffset + (OpponentHolder ? (Vector3.up * _commandHolder.SpaceBetweenCards) : (Vector3.down * _commandHolder.SpaceBetweenCards));
			for (int num2 = 0; num2 < currentOffset; num2++)
			{
				CardStack cardStack2 = CardStacks[num2];
				allData.Add(new CardLayoutData(cardStack2.StackParent, vector + center, Quaternion.identity, Vector3.one, isVisibleInLayout: false));
				for (int num3 = 0; num3 < cardStack2.StackedCards.Count; num3++)
				{
					DuelScene_CDC card = cardStack2.StackedCards[num3];
					allData.Add(new CardLayoutData(card, vector + center, Quaternion.identity, Vector3.one, isVisibleInLayout: false));
				}
			}
			vector = (OpponentHolder ? Vector3.up : Vector3.down) * _commandHolder.StartCardYPosition;
			for (int num4 = currentOffset; num4 < num; num4++)
			{
				Vector3 vector2 = vector + center;
				Quaternion rot = Quaternion.identity * rotation;
				CardStack cardStack3 = CardStacks[num4];
				cardStack3.IsVisible = true;
				allData.Add(new CardLayoutData(cardStack3.StackParent, vector2, rot));
				Vector3 pos = vector2 + Vector3.forward * 0.5f;
				for (int num5 = 1; num5 < cardStack3.StackedCards.Count; num5++)
				{
					DuelScene_CDC card2 = cardStack3.StackedCards[num5];
					allData.Add(new CardLayoutData(card2, pos, rot, Vector3.one, isVisibleInLayout: false));
					pos += Vector3.forward * 0.5f;
				}
				if (OpponentHolder)
				{
					vector += Vector3.up * _commandHolder.SpaceBetweenCards;
				}
				else
				{
					vector += Vector3.down * _commandHolder.SpaceBetweenCards;
				}
			}
			vector = Vector3.left * _commandHolder.OffscreenXOffset + (OpponentHolder ? (Vector3.up * _commandHolder.SpaceBetweenCards * _commandHolder.MaxCardsShown) : (Vector3.down * _commandHolder.SpaceBetweenCards * _commandHolder.MaxCardsShown));
			for (int num6 = num; num6 < CardStacks.Count; num6++)
			{
				CardStack cardStack4 = CardStacks[num6];
				allData.Add(new CardLayoutData(cardStack4.StackParent, vector + center, Quaternion.identity, Vector3.one, isVisibleInLayout: false));
				for (int num7 = 0; num7 < cardStack4.StackedCards.Count; num7++)
				{
					DuelScene_CDC card3 = cardStack4.StackedCards[num7];
					allData.Add(new CardLayoutData(card3, vector + center, Quaternion.identity, Vector3.one, isVisibleInLayout: false));
				}
			}
		}

		private void PageUp()
		{
			_currentOffset -= _commandHolder.MovePerPagePress;
			_currentOffset = Mathf.Max(_currentOffset, 0);
			_commandHolder.LayoutNow();
		}

		private void PageDown()
		{
			_currentOffset += _commandHolder.MovePerPagePress;
			_currentOffset = Mathf.Min(_currentOffset, _commandHolder._cardViews.Count - _commandHolder.MaxCardsShown);
			_commandHolder.LayoutNow();
		}

		private int CardViewComparator(DuelScene_CDC lhs, DuelScene_CDC rhs)
		{
			int num = lhs.Model.GrpId.CompareTo(rhs.Model.GrpId);
			if (num == 0)
			{
				num = lhs.Model.TitleId.CompareTo(rhs.Model.TitleId);
			}
			if (num == 0)
			{
				num = lhs.Model.InstanceId.CompareTo(rhs.Model.InstanceId);
			}
			return num;
		}
	}

	public class CardStack
	{
		public readonly List<DuelScene_CDC> StackedCards = new List<DuelScene_CDC>();

		private GameManager _gameManager;

		public DuelScene_CDC StackParent { get; private set; }

		public ICardDataAdapter StackParentModel => StackParent.Model;

		public int StackCount => StackedCards.Count;

		public bool IsVisible { get; set; }

		public CardStack(DuelScene_CDC parent, GameManager gameManager)
		{
			StackParent = parent;
			AddCardToStack(parent);
			_gameManager = gameManager;
		}

		public bool CanStack(AssetLookupSystem assetLookupSystem, ICardDataAdapter cardModel)
		{
			MtgCardInstance instance = StackParentModel.Instance;
			MtgCardInstance instance2 = cardModel.Instance;
			if (instance == null)
			{
				return false;
			}
			if (instance2 == null)
			{
				return false;
			}
			if (instance.ObjectType != instance2.ObjectType)
			{
				return false;
			}
			assetLookupSystem.Blackboard.Clear();
			assetLookupSystem.Blackboard.SetCardDataExtensive(cardModel);
			if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<CanStackOverride> loadedTree))
			{
				CanStackOverride payload = loadedTree.GetPayload(assetLookupSystem.Blackboard);
				if (payload != null)
				{
					if (payload.StackMiniCDC)
					{
						return StackParentModel.GrpId == cardModel.GrpId;
					}
					return false;
				}
			}
			if (instance.ObjectType != GameObjectType.Emblem)
			{
				if (instance.ParentId != instance2.ParentId)
				{
					if (instance.Parent == null || instance2.Parent == null)
					{
						if (instance.ObjectSourceGrpId != instance2.ObjectSourceGrpId)
						{
							return false;
						}
					}
					else
					{
						if (instance.Parent.GrpId != instance2.Parent.GrpId)
						{
							return false;
						}
						if (!instance.Parent.CastingTimeOptions.ContainSame(instance2.Parent.CastingTimeOptions))
						{
							return false;
						}
					}
				}
				if (StackParentModel?.RulesTextOverride?.GetOverride(CardTextColorSettings.DEFAULT) != cardModel.RulesTextOverride?.GetOverride(CardTextColorSettings.DEFAULT))
				{
					return false;
				}
			}
			return CardViewUtilities.IsSame(StackParentModel, cardModel, _gameManager);
		}

		public void AddCardToStack(DuelScene_CDC cardView)
		{
			StackedCards.Add(cardView);
		}
	}

	[Header("Layout Data")]
	[SerializeField]
	private CdcStackCounterView StackCounterPrefab;

	[SerializeField]
	private float StackCounterScale = 1.3f;

	[SerializeField]
	private RegionPagingButton UpPagingButtonPrefab;

	[SerializeField]
	private RegionPagingButton DownPagingButtonPrefab;

	[SerializeField]
	private Vector3 _pageButtonOffset = new Vector3(0f, 2.15f, 0f);

	[SerializeField]
	private Vector2 PagingButtonSize = new Vector2(3f, 2f);

	[SerializeField]
	private Vector3 _cardScaleOverride = Vector3.one;

	[SerializeField]
	private int MaxCardsShown = 3;

	[SerializeField]
	private int MovePerPagePress = 1;

	[SerializeField]
	private float StartCardYPosition = 2.15f;

	[SerializeField]
	private float SpaceBetweenCards = 2.15f;

	[SerializeField]
	private float OffscreenXOffset = 3f;

	[Header("Docking")]
	[SerializeField]
	private Vector3 _endPosition;

	[SerializeField]
	private Canvas _dockingButtonCanvas;

	[SerializeField]
	private CustomButton _dockButton;

	[SerializeField]
	private CustomButton _undockButton;

	[SerializeField]
	private bool _useDockButton = true;

	private bool _isDocked;

	private CardLayout_Command _layout;

	private List<CdcStackCounterView> _activeStackCounters = new List<CdcStackCounterView>();

	private IUnityObjectPool _objectPool;

	private HashSet<DuelScene_CDC> _miniCdcs = new HashSet<DuelScene_CDC>();

	private readonly List<MtgCardInstance> _addCardExistingInstances = new List<MtgCardInstance>();

	public bool UsingPagingButtons => base.CardViews.Count > MaxCardsShown;

	private void Awake()
	{
		_objectPool = Pantry.Get<IUnityObjectPool>();
		_dockButton.gameObject.SetActive(value: false);
		_undockButton.gameObject.SetActive(value: false);
		_dockButton.OnClick.AddListener(delegate
		{
			Dock(dockIt: true);
		});
		_undockButton.OnClick.AddListener(delegate
		{
			Dock(dockIt: false);
		});
	}

	public override void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		base.Init(gameManager, cardViewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
		base.Layout = (_layout = new CardLayout_Command(this, UpPagingButtonPrefab, DownPagingButtonPrefab, _gameManager, _assetLookupSystem));
	}

	protected override void OnDestroy()
	{
		_dockButton.OnClick.RemoveAllListeners();
		_undockButton.OnClick.RemoveAllListeners();
		foreach (CdcStackCounterView activeStackCounter in _activeStackCounters)
		{
			activeStackCounter.Cleanup();
			_objectPool.PushObject(activeStackCounter.gameObject);
		}
		_activeStackCounters.Clear();
		base.OnDestroy();
	}

	protected override void LayoutNowInternal(List<DuelScene_CDC> cardsToLayout, bool layoutInstantly = false)
	{
		base.LayoutNowInternal(cardsToLayout, layoutInstantly);
		List<CardStack> list = _layout.CardStacks.FindAll((CardStack x) => x.IsVisible && x.StackCount > 1);
		int count = list.Count;
		while (count != _activeStackCounters.Count)
		{
			if (count > _activeStackCounters.Count)
			{
				CdcStackCounterView component = _objectPool.PopObject(StackCounterPrefab.gameObject).GetComponent<CdcStackCounterView>();
				Transform obj = component.transform;
				obj.SetParent(base.transform);
				obj.localScale = Vector3.one * StackCounterScale;
				obj.localRotation = Quaternion.identity;
				_activeStackCounters.Add(component);
			}
			else
			{
				CdcStackCounterView cdcStackCounterView = _activeStackCounters[0];
				_activeStackCounters.RemoveAt(0);
				cdcStackCounterView.Cleanup();
				_objectPool.PushObject(cdcStackCounterView.gameObject);
			}
		}
		for (int num = 0; num < count; num++)
		{
			CardStack cardStack = list[num];
			CdcStackCounterView cdcStackCounterView2 = _activeStackCounters[num];
			cdcStackCounterView2.transform.position = getCounterPosition(cardStack.StackParent);
			cdcStackCounterView2.SetCount(cardStack.StackCount);
		}
		Vector3 getCounterPosition(DuelScene_CDC cardView)
		{
			Vector3 size = cardView.Collider.size;
			Vector3 vector = _cardScaleOverride * _cdcSize;
			float x = size.x * vector.x;
			float y = size.y * vector.y;
			float z = size.z * vector.z - 0.2f;
			Vector3 vector2 = new Vector3(x, y, z) / 2f;
			CardLayoutData cardLayoutData = _previousLayoutData.Find((CardLayoutData cardLayoutData2) => cardLayoutData2.Card == cardView);
			return base.transform.TransformPoint(cardLayoutData.Position + vector2);
		}
	}

	private void UpdateButtons()
	{
		bool flag = !_isDocked && base.CardViews.Count > 0;
		bool flag2 = _isDocked && base.CardViews.Count > 0;
		if (_useDockButton)
		{
			if (_dockButton.gameObject.activeSelf != flag)
			{
				_dockButton.gameObject.SetActive(flag);
			}
			if (_undockButton.gameObject.activeSelf != flag2)
			{
				_undockButton.gameObject.SetActive(flag2);
			}
		}
		_layout.ShowPagingButtons = flag;
	}

	public void Dock(bool dockIt)
	{
		_isDocked = dockIt;
		LayoutNow();
		UpdateButtons();
	}

	protected override Vector3 GetLayoutCenterPoint()
	{
		if (!_isDocked)
		{
			return base.GetLayoutCenterPoint();
		}
		return _endPosition;
	}

	protected override void OnPreLayout()
	{
		if (base.CardViews.Count == 0)
		{
			_isDocked = false;
		}
		base.OnPreLayout();
		UpdateButtons();
	}

	public override IdealPoint GetLayoutEndpoint(CardLayoutData data)
	{
		Transform parent = data.Card.Root.parent;
		return new IdealPoint(parent.TransformPoint(data.Position), parent.rotation * data.Rotation, _overrideCdcSize ? (_cardScaleOverride * _cdcSize) : data.Scale);
	}

	protected override SplineEventData GetLayoutSplineEvents(CardLayoutData data)
	{
		SplineEventData layoutSplineEvents = base.GetLayoutSplineEvents(data);
		if (data == null || data.Card == null || data.CardGameObject == null)
		{
			Debug.LogErrorFormat("Null card passed into CommandCardHolder.GetLayoutSplineEvents()!");
			return layoutSplineEvents;
		}
		CardStack stackForCard = GetStackForCard(data.Card);
		if (stackForCard == null)
		{
			return layoutSplineEvents;
		}
		if (!stackForCard.IsVisible)
		{
			return layoutSplineEvents;
		}
		DuelScene_CDC parentEmblemView = stackForCard.StackParent;
		if (parentEmblemView == null || parentEmblemView.Model == null)
		{
			return layoutSplineEvents;
		}
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCardDataExtensive(data.Card.Model);
		_assetLookupSystem.Blackboard.CardHolderType = CardHolderType.Command;
		AssetLookupTree<EtcSFX> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<EtcSFX>();
		AssetLookupTree<EtcVFX> assetLookupTree2 = _assetLookupSystem.TreeLoader.LoadTree<EtcVFX>();
		EtcSFX payload = assetLookupTree.GetPayload(_assetLookupSystem.Blackboard);
		EtcVFX payload2 = assetLookupTree2.GetPayload(_assetLookupSystem.Blackboard);
		bool flag = false;
		if (payload2 != null)
		{
			foreach (VfxData vfxData in payload2.VfxDatas)
			{
				DuelScene_CDC cacheCard = data.Card;
				float time = Mathf.Clamp01(vfxData.PrefabData.StartTime);
				if (payload != null && !flag)
				{
					flag = true;
					layoutSplineEvents.Events.Add(new SplineEventAudio(time, payload.SfxData.AudioEvents, cacheCard.Root.gameObject));
				}
				if (vfxData.PrefabData == null)
				{
					continue;
				}
				layoutSplineEvents.Events.Add(new SplineEventCallback(time, delegate
				{
					if (!(cacheCard == null) && !(cacheCard.EffectsRoot == null))
					{
						_vfxProvider.PlayVFX(vfxData, cacheCard.Model, cacheCard.Model.Instance, parentEmblemView.EffectsRoot);
					}
				}));
			}
		}
		else
		{
			if (payload != null)
			{
				layoutSplineEvents.Events.Add(new SplineEventAudio(0f, payload.SfxData.AudioEvents, data.Card.Root.gameObject));
			}
			Debug.LogWarningFormat("Card \"{0}\" entered the command zone with no ETC VFX. That's pretty lame, add some.", data.Card.name);
		}
		return layoutSplineEvents;
	}

	public void AddGameEffect(DuelScene_CDC miniCdc, GameEffectType effectType)
	{
		_miniCdcs.AddIfNotNull(miniCdc);
		if (effectType == GameEffectType.Qualification || (uint)(effectType - 4) <= 4u)
		{
			base.AddCard(miniCdc);
		}
		else
		{
			AddCard(miniCdc);
		}
	}

	public IEnumerable<DuelScene_CDC> GetAllGameEffects()
	{
		return _miniCdcs;
	}

	public override void AddCard(DuelScene_CDC newCardView)
	{
		_addCardExistingInstances.Clear();
		foreach (DuelScene_CDC cardView in _cardViews)
		{
			_addCardExistingInstances.Add(CardViewUtilities.InstanceForCardView(cardView));
		}
		if (Wotc.Mtga.DuelScene.Command.Utils.CanAddCard(CardViewUtilities.InstanceForCardView(newCardView), _addCardExistingInstances))
		{
			base.AddCard(newCardView);
		}
	}

	public override void RemoveCard(DuelScene_CDC cdc)
	{
		_miniCdcs.Remove(cdc);
		base.RemoveCard(cdc);
	}

	public CardStack GetStackForCard(DuelScene_CDC cdc)
	{
		return _layout.CardStacks.Find((CardStack x) => x.StackedCards.Contains(cdc));
	}
}
