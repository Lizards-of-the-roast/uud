using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Ability;
using AssetLookupTree.Payloads.Card;
using AssetLookupTree.Payloads.RegionTransfer;
using GreClient.CardData;
using GreClient.Rules;
using MovementSystem;
using Pooling;
using UnityEngine;
using Wotc.Mtga.Duel;
using Wotc.Mtga.DuelScene.Battlefield;
using Wotc.Mtga.DuelScene.Companions;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Universal;

public class UniversalBattlefieldCardHolder : ZoneCardHolderBase, IBattlefieldCardHolder, ICardHolder
{
	private Phase _lastActivePhase;

	private Step _lastActiveStep;

	[Header("Stacking")]
	[SerializeField]
	private GameObject stackUiPrefab;

	[SerializeField]
	private Vector3 stackUiOffset = Vector3.zero;

	[Header("Tapped and Attacking")]
	[SerializeField]
	private Vector3 _tapRotation = new Vector3(0f, 0f, -8f);

	[SerializeField]
	private Vector3 _declaredAttackOffset = new Vector3(0f, 0.3f, 0f);

	[Header("Region dragging")]
	[SerializeField]
	private float _dragScale = 1f;

	[SerializeField]
	private List<UniversalBattlefieldRegion> _regions;

	private HashSet<IBattlefieldStack> _handledEtbStacks = new HashSet<IBattlefieldStack>();

	private HashSet<IBattlefieldStack> _stacksToRefreshInLateUpdate = new HashSet<IBattlefieldStack>();

	public bool LayoutLocked { get; set; }

	public List<CardLayoutData> PreviousLayoutData => _previousLayoutData;

	public Transform Transform => base.transform;

	public override void Init(GameManager gameManager, ICardViewManager cardViewManager, ISplineMovementSystem splineMovementSystem, CardViewBuilder cardViewBuilder, IClientLocProvider locMan, MatchManager matchManager)
	{
		base.Init(gameManager, cardViewManager, splineMovementSystem, cardViewBuilder, locMan, matchManager);
		IContext context = gameManager.Context;
		IEqualityComparer<DuelScene_CDC> equalityComparer = new CanStackComparer(context.Get<IObjectPool>(), context.Get<IGameStateProvider>(), context.Get<IWorkflowProvider>(), context.Get<ICardViewProvider>(), gameManager.ReferenceMapAggregate, gameManager.UIManager);
		_regions.ForEach(delegate(UniversalBattlefieldRegion region)
		{
			region.Init(_tapRotation, _declaredAttackOffset, cardViewManager, gameManager.GenericPool, gameManager.MatchManager.PlayerInfoForNum, gameManager.AssetLookupSystem, gameManager.CardHolderManager, equalityComparer);
		});
		base.Layout = new UniversalBattlefieldLayout(_regions, gameManager, stackUiPrefab, stackUiOffset);
		if (_cardViewBuilder != null)
		{
			_cardViewBuilder.onCardUpdated += OnCardUpdated;
		}
	}

	protected override void OnDestroy()
	{
		if (base.Layout is UniversalBattlefieldLayout universalBattlefieldLayout)
		{
			universalBattlefieldLayout.Clear();
		}
		if (_cardViewBuilder != null)
		{
			_cardViewBuilder.onCardUpdated -= OnCardUpdated;
		}
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			region.Dispose();
		}
		base.OnDestroy();
	}

	protected override void OnDrawGizmos()
	{
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			region.DrawGizmos();
		}
	}

	protected override void LateUpdate()
	{
		if (_stacksToRefreshInLateUpdate.Count > 0)
		{
			foreach (IBattlefieldStack item in _stacksToRefreshInLateUpdate)
			{
				item?.RefreshAbilitiesBasedOnStackPosition();
			}
			_stacksToRefreshInLateUpdate.Clear();
		}
		if (!LayoutLocked)
		{
			base.LateUpdate();
		}
	}

	protected override void OnPreLayout()
	{
		base.OnPreLayout();
		_handledEtbStacks.Clear();
	}

	protected override void OnPostLayout()
	{
		base.OnPostLayout();
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			foreach (IUniversalBattlefieldGroup allGroup in region.AllGroups)
			{
				foreach (UniversalBattlefieldStack visibleStack in allGroup.VisibleStacks)
				{
					visibleStack?.RefreshAbilitiesBasedOnStackPosition();
				}
			}
		}
		UpdatePetPositions();
		foreach (DuelScene_CDC key in (base.Layout as UniversalBattlefieldLayout).IntraGroupChanges.Keys)
		{
			if (HandleCardDragEnd(key))
			{
				LayoutNow();
			}
		}
	}

	public override IdealPoint GetLayoutEndpoint(CardLayoutData data)
	{
		return new IdealPoint(data.Position, base.transform.rotation * data.Rotation, data.Scale);
	}

	protected override SplineEventData GetLayoutSplineEvents(CardLayoutData data)
	{
		SplineEventData layoutSplineEvents = base.GetLayoutSplineEvents(data);
		if (data == null || data.Card == null || data.CardGameObject == null)
		{
			Debug.LogErrorFormat("Null card passed into BattlefieldCardHolder.GetLayoutSplineEvents()!");
			return layoutSplineEvents;
		}
		IBattlefieldStack stackForCard = GetStackForCard(data.Card);
		bool num = data.Card.Model.Zone.Type == ZoneType.Battlefield && data.Card.PreviousCardHolder.CardHolderType != CardHolderType.Hand;
		bool flag = _gameManager.ViewManager.GetCardPreviousId(data.Card.InstanceId) != data.Card.InstanceId && data.Card.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield && data.Card.PreviousCardHolder.CardHolderType == CardHolderType.Hand;
		bool flag2 = data.Card.Model.Zone.Type == ZoneType.Exile && data.Card.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield;
		if (num || flag)
		{
			foreach (DuelScene_CDC cardView in base.CardViews)
			{
				_vfxProvider.GenerateEtbTriggerEvents(cardView, layoutSplineEvents.Events, stackForCard, cardView.EffectsRoot);
			}
			layoutSplineEvents.Events.AddRange(_vfxProvider.GenerateEtbSplineEvents(data.Card, stackForCard, !_handledEtbStacks.Contains(stackForCard), data.Card.EffectsRoot));
			layoutSplineEvents.Events.Add(new SplineEventCallbackWithParams<IReadOnlyCollection<DuelScene_CDC>>(1f, _cardViews, delegate(float _, IReadOnlyCollection<DuelScene_CDC> cdcList)
			{
				foreach (DuelScene_CDC cdc in cdcList)
				{
					cdc.PlayReactionAnimation(CardReactionEnum.ETB);
				}
			}));
		}
		else if (flag2)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_basicloc_exile, data.Card.gameObject);
		}
		_handledEtbStacks.Add(stackForCard);
		return layoutSplineEvents;
	}

	protected override string GetInternalLayoutSplinePath(CardLayoutData data)
	{
		string text = base.GetInternalLayoutSplinePath(data);
		if (string.IsNullOrEmpty(text) && (base.Layout as UniversalBattlefieldLayout).IntraGroupChanges.TryGetValue(data.Card, out (IUniversalBattlefieldGroup, IUniversalBattlefieldGroup) value))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(data.Card.Model);
			_assetLookupSystem.Blackboard.CardHolderType = CardHolderType.Battlefield;
			_assetLookupSystem.Blackboard.RegionPair = new RegionPair(value.Item1?.Config.RegionType ?? BattlefieldRegionType.None, value.Item1?.Config.RegionController ?? GREPlayerNum.Invalid, value.Item2?.Config.RegionType ?? BattlefieldRegionType.None, value.Item2?.Config.RegionController ?? GREPlayerNum.Invalid);
			if (_assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<InternalMovementPayload_Spline> loadedTree))
			{
				InternalMovementPayload_Spline payload = loadedTree.GetPayload(_assetLookupSystem.Blackboard);
				if (payload != null)
				{
					text = payload.SplineDataRef.RelativePath;
				}
			}
			_assetLookupSystem.Blackboard.Clear();
		}
		return text;
	}

	protected override SplineEventData GetInternalLayoutSplineEvents(CardLayoutData data)
	{
		SplineEventData splineEvents = base.GetInternalLayoutSplineEvents(data);
		CardHolderBase.SetupSplineEventsALT<InternalMovementPayload_VFX, InternalMovementPayload_SFX>(data, added: false, ref splineEvents, GetCardPosition(data.Card), CardViewUtilities.GetFromToZoneForCard(data.Card, added: false), _gameManager);
		if ((base.Layout as UniversalBattlefieldLayout).IntraGroupChanges.TryGetValue(data.Card, out (IUniversalBattlefieldGroup, IUniversalBattlefieldGroup) value))
		{
			_assetLookupSystem.Blackboard.Clear();
			_assetLookupSystem.Blackboard.SetCardDataExtensive(data.Card.Model);
			_assetLookupSystem.Blackboard.CardHolderType = CardHolderType.Battlefield;
			_assetLookupSystem.Blackboard.RegionPair = new RegionPair(value.Item1?.Config.RegionType ?? BattlefieldRegionType.None, value.Item1?.Config.RegionController ?? GREPlayerNum.Invalid, value.Item2?.Config.RegionType ?? BattlefieldRegionType.None, value.Item2?.Config.RegionController ?? GREPlayerNum.Invalid);
			AssetLookupTree<RegionSFX> assetLookupTree = _assetLookupSystem.TreeLoader.LoadTree<RegionSFX>();
			AssetLookupTree<RegionVFX> assetLookupTree2 = _assetLookupSystem.TreeLoader.LoadTree<RegionVFX>();
			RegionSFX payload = assetLookupTree.GetPayload(_assetLookupSystem.Blackboard);
			RegionVFX payload2 = assetLookupTree2.GetPayload(_assetLookupSystem.Blackboard);
			if (payload2 != null)
			{
				foreach (VfxData vfxData in payload2.VfxDatas)
				{
					float time = Mathf.Clamp01(vfxData.PrefabData.StartTime);
					if (payload != null)
					{
						splineEvents.Events.Add(new SplineEventAudio(time, payload.SfxData.AudioEvents, data.CardGameObject));
					}
					if (vfxData.PrefabData == null)
					{
						continue;
					}
					DuelScene_CDC cacheCard = data.Card;
					VfxData cachedVfxData = vfxData;
					splineEvents.Events.Add(new SplineEventCallback(time, delegate
					{
						if ((bool)cacheCard && (bool)cacheCard.EffectsRoot)
						{
							_vfxProvider.PlayVFX(cachedVfxData, cacheCard.Model);
						}
					}));
				}
				if (base.CardViews != null && data.Card.Model.Instance != null && data.Card.Model.Instance.AttachedToId != 0)
				{
					splineEvents.Events.Add(new SplineEventCallback(1f, delegate
					{
						base.CardViews.FirstOrDefault((DuelScene_CDC x) => x.InstanceId == data.Card.Model.Instance.AttachedToId)?.PlayReactionAnimation(CardReactionEnum.Attachment);
					}));
				}
			}
		}
		return splineEvents;
	}

	private void OnCardUpdated(BASE_CDC cardView)
	{
		ICardDataAdapter model = cardView.Model;
		if (model != null && model.Zone?.Type == ZoneType.Battlefield && cardView is DuelScene_CDC card)
		{
			IBattlefieldStack stackForCard = GetStackForCard(card);
			if (stackForCard != null)
			{
				_stacksToRefreshInLateUpdate.Add(stackForCard);
			}
		}
	}

	public bool HandleCardClick(DuelScene_CDC cardView)
	{
		bool layoutStale;
		bool result = (base.Layout as UniversalBattlefieldLayout).HandleCardClick(cardView, _gameManager.CurrentInteraction, out layoutStale);
		if (layoutStale)
		{
			LayoutNow();
		}
		return result;
	}

	public bool HandleCardDragBegin(DuelScene_CDC cardView, Vector2 pointerPos)
	{
		return (base.Layout as UniversalBattlefieldLayout).HandleCardDragBegin(cardView, pointerPos);
	}

	public bool HandleCardDragSustain(DuelScene_CDC cardView, Vector2 pointerPos)
	{
		if ((base.Layout as UniversalBattlefieldLayout).HandleCardDragSustain(cardView, pointerPos, _dragScale, out var cardLayoutDatas))
		{
			foreach (CardLayoutData item in cardLayoutDatas)
			{
				ApplyLayoutData(item, added: false, CalcCardVisibility(item, 0));
			}
			UpdatePetPositions();
			return true;
		}
		return false;
	}

	public bool HandleCardDragEnd(DuelScene_CDC cardView)
	{
		if ((base.Layout as UniversalBattlefieldLayout).HandleCardDragEnd(cardView))
		{
			LayoutNow();
			return true;
		}
		return false;
	}

	public bool CardIsStackParent(DuelScene_CDC card)
	{
		if (card == null)
		{
			return false;
		}
		IBattlefieldStack stackForCard = GetStackForCard(card);
		if (stackForCard == null)
		{
			return false;
		}
		return stackForCard.StackParent == card;
	}

	public Transform GetRegionTransformForCardType(CardType cardType, GREPlayerNum playerNum)
	{
		MtgCardInstance mtgCardInstance = _gameManager.GenericPool.PopObject<MtgCardInstance>();
		mtgCardInstance.Controller = _gameManager.CurrentGameState.GetPlayerByEnum(playerNum);
		mtgCardInstance.CardTypes.Add(cardType);
		CardPrintingData blank = CardPrintingData.Blank;
		ICardDataAdapter cardModel = new CardData(mtgCardInstance, blank);
		Transform result = null;
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			if (region.CardIsValid(cardModel, _gameManager.CurrentGameState.Players))
			{
				result = region.Transform;
				break;
			}
		}
		mtgCardInstance.Controller = null;
		mtgCardInstance.CardTypes.Clear();
		_gameManager.GenericPool.PushObject(mtgCardInstance);
		return result;
	}

	public IBattlefieldStack GetStackForCard(DuelScene_CDC card)
	{
		return GetStackForInstanceId(card.InstanceId);
	}

	public IBattlefieldStack GetStackForInstanceId(uint id)
	{
		foreach (UniversalBattlefieldStack item in _regions.SelectMany((UniversalBattlefieldRegion region) => region.AllGroups).SelectMany((IUniversalBattlefieldGroup group) => group.VisibleStacks))
		{
			foreach (DuelScene_CDC allCard in item.AllCards)
			{
				if (allCard.InstanceId == id)
				{
					return item;
				}
			}
		}
		return null;
	}

	public void RefreshAbilitiesForCardsStack(DuelScene_CDC card)
	{
		IBattlefieldStack stackForCard = GetStackForCard(card);
		if (stackForCard != null)
		{
			_stacksToRefreshInLateUpdate.Add(stackForCard);
		}
	}

	protected override void HandleAddedCard(DuelScene_CDC cardView)
	{
		foreach (CDCPart value in cardView.ActiveParts.Values)
		{
			value.OnPhaseUpdate(_lastActivePhase);
		}
		base.HandleAddedCard(cardView);
	}

	public override void RemoveCard(DuelScene_CDC cardView)
	{
		if (_cardViews.Contains(cardView))
		{
			base.RemoveCard(cardView);
			cardView.UpdateAbilityVisuals<PersistVFX, PersistSfx>(display: false, animate: false, null);
			cardView.UpdateTopCardRelevantVisuals(display: true);
			cardView.UpdateCounterVisibility(display: true);
		}
	}

	public void UpdateForPhase(Phase phase, Step step)
	{
		if (_lastActivePhase == phase && _lastActiveStep == step)
		{
			return;
		}
		_lastActivePhase = phase;
		_lastActiveStep = step;
		foreach (UniversalBattlefieldStack item in _regions.SelectMany((UniversalBattlefieldRegion region) => region.AllGroups).SelectMany((IUniversalBattlefieldGroup group) => group.VisibleStacks))
		{
			_stacksToRefreshInLateUpdate.Add(item);
			foreach (DuelScene_CDC allCard in item.AllCards)
			{
				if (allCard == null || allCard.ActiveParts == null)
				{
					continue;
				}
				foreach (CDCPart value in allCard.ActiveParts.Values)
				{
					value.OnPhaseUpdate(phase);
				}
			}
		}
		if ((base.Layout as UniversalBattlefieldLayout).HandlePhaseChange())
		{
			LayoutNow();
		}
	}

	public void SetOpponentFocus(params uint[] playerIds)
	{
		(base.Layout as UniversalBattlefieldLayout).FocusPlayerIds = (IReadOnlyCollection<uint>)(object)playerIds;
		LayoutNow();
	}

	private void UpdatePetPositions()
	{
		if (_gameManager == null)
		{
			return;
		}
		ICompanionViewProvider companionViewProvider = _gameManager.Context.Get<ICompanionViewProvider>() ?? NullCompanionViewProvider.Default;
		foreach (UniversalBattlefieldRegion region in _regions)
		{
			foreach (IUniversalBattlefieldGroup allGroup in region.AllGroups)
			{
				if (!(allGroup is UniversalBattlefieldPet universalBattlefieldPet))
				{
					continue;
				}
				AccessoryController companionByPlayerType = companionViewProvider.GetCompanionByPlayerType(universalBattlefieldPet.Config.RegionController);
				if ((bool)companionByPlayerType && (bool)companionByPlayerType.transform)
				{
					Transform parent = companionByPlayerType.transform.parent;
					if ((object)parent != null)
					{
						_splineMovementSystem.AddPermanentGoal(parent, new IdealPoint(universalBattlefieldPet.Position, parent.rotation, parent.localScale));
					}
				}
			}
		}
	}
}
