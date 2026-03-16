using System;
using System.Collections.Generic;
using AssetLookupTree;
using GreClient.Rules;
using Pooling;
using ReferenceMap;
using UnityEngine;
using WorkflowVisuals;
using Wotc;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.DuelScene.Interactions;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Unity;
using Wotc.Mtgo.Gre.External.Messaging;

[Serializable]
public class IntentionLineManager : IDisposable
{
	private const string KEY_DEFAULT = "Default";

	private const string KEY_DEFAULT_TARGET = "Target";

	private const string KEY_DEFAULT_ABILITY = "Ability";

	private const string KEY_DEFAULT_COMBAT = "Combat";

	private const string KEY_DEFAULT_LINK = "Link";

	private readonly CombatAnimationPlayer _combatAnimationPlayer;

	private readonly Transform _arrowRoot;

	private readonly IObjectPool _objectPool;

	private readonly IUnityObjectPool _unityPool;

	private readonly MapAggregate _referenceMap;

	private readonly IGameStateProvider _gameStateProvider;

	private readonly IWorkflowProvider _workflowProvider;

	private readonly IEntityViewProvider _entityViewProvider;

	private readonly BrowserManager _browserManager;

	private readonly UXEventQueue _uxEventQueue;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly ISignalListen<CardHolderCreatedSignalArgs> _cardHolderCreatedEvent;

	private readonly HashSet<MtgCardInstance> _attackingCards;

	private readonly HashSet<MtgCardInstance> _unblockedAttackers;

	private readonly HashSet<MtgCardInstance> _blockingCards;

	private readonly HashSet<uint> _attackQuarries;

	private readonly HashSet<(uint, uint)> _activeCombatArrows;

	private readonly Dictionary<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> _activeArrowsGameState;

	private readonly Dictionary<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> _activeArrowsWorkflow;

	private readonly List<FromEntityIntentionBase> _toRemoveBuffer;

	private readonly Dictionary<Type, Queue<FromEntityIntentionBase>> _mediatorPool;

	private readonly HashSet<Arrows.LineData> _suppressedLineData;

	private IAvatarView _hoveredAvatarView;

	private DuelScene_CDC _hoveredCardView;

	private BrowserBase _shownBrowser;

	private bool _inCombatUxEvent;

	private bool _exclusiveWorkflowArrows;

	private bool _prevWorkflowArrowsHadCardToMouse;

	private StackCardHolder _stack;

	private IBattlefieldCardHolder _battlefield;

	public IntentionLineManager(IObjectPool objectPool, IUnityObjectPool unityPool, MapAggregate referenceMap, UXEventQueue eventQueue, IGameStateProvider gameStateProvider, IWorkflowProvider workflowProvider, IEntityViewProvider entityViewProvider, ISignalListen<CardHolderCreatedSignalArgs> cardHolderCreatedEvent, CombatAnimationPlayer combatAnimationPlayer, BrowserManager browserManager, AssetLookupSystem assetLookupSystem, Transform arrowRoot)
	{
		_objectPool = objectPool ?? NullObjectPool.Default;
		_unityPool = unityPool ?? NullUnityObjectPool.Default;
		_referenceMap = referenceMap ?? new MapAggregate();
		_uxEventQueue = eventQueue ?? new UXEventQueue();
		_workflowProvider = workflowProvider ?? NullWorkflowProvider.Default;
		_gameStateProvider = gameStateProvider ?? NullGameStateProvider.Default;
		_entityViewProvider = entityViewProvider ?? NullEntityViewProvider.Default;
		_cardHolderCreatedEvent = cardHolderCreatedEvent;
		_combatAnimationPlayer = combatAnimationPlayer;
		_assetLookupSystem = assetLookupSystem;
		_arrowRoot = arrowRoot;
		_uxEventQueue.EventExecutionCommenced += OnUxEventCommenced;
		_uxEventQueue.EventExecutionCompleted += OnUxEventCompleted;
		_browserManager = browserManager;
		if (_browserManager != null)
		{
			_browserManager.BrowserShown += OnBrowserShown;
			_browserManager.BrowserHidden += OnBrowserHidden;
		}
		CardHoverController.OnHoveredCardUpdated += OnHoveredCardChanged;
		CardHoverController.HoveredAvatarChangedHandlers += OnHoveredAvatarChanged;
		_cardHolderCreatedEvent.Listeners += OnCardHolderCreated;
		_attackingCards = _objectPool.PopObject<HashSet<MtgCardInstance>>();
		_unblockedAttackers = _objectPool.PopObject<HashSet<MtgCardInstance>>();
		_blockingCards = _objectPool.PopObject<HashSet<MtgCardInstance>>();
		_attackQuarries = _objectPool.PopObject<HashSet<uint>>();
		_activeCombatArrows = _objectPool.PopObject<HashSet<(uint, uint)>>();
		_toRemoveBuffer = _objectPool.PopObject<List<FromEntityIntentionBase>>();
		_mediatorPool = _objectPool.PopObject<Dictionary<Type, Queue<FromEntityIntentionBase>>>();
		_activeArrowsGameState = _objectPool.PopObject<Dictionary<FromEntityIntentionBase, DreamteckIntentionArrowBehavior>>();
		_activeArrowsWorkflow = _objectPool.PopObject<Dictionary<FromEntityIntentionBase, DreamteckIntentionArrowBehavior>>();
		_suppressedLineData = _objectPool.PopObject<HashSet<Arrows.LineData>>();
	}

	private void OnCardHolderCreated(CardHolderCreatedSignalArgs args)
	{
		if (args != null)
		{
			ICardHolder cardHolder = args.CardHolder;
			if (cardHolder is StackCardHolder stack)
			{
				_stack = stack;
			}
			else if (cardHolder is IBattlefieldCardHolder battlefield)
			{
				_battlefield = battlefield;
				_battlefield.OnCardHolderUpdated += OnBattlefieldLaidOut;
			}
		}
	}

	public void Update()
	{
		updateEachArrow(_activeArrowsGameState, _toRemoveBuffer);
		updateEachArrow(_activeArrowsWorkflow, _toRemoveBuffer);
		void updateEachArrow(Dictionary<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> activeArrows, List<FromEntityIntentionBase> toRemoveBuffer)
		{
			toRemoveBuffer.Clear();
			foreach (KeyValuePair<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> activeArrow in activeArrows)
			{
				FromEntityIntentionBase key = activeArrow.Key;
				if (!key.ArrowStillValid)
				{
					toRemoveBuffer.Add(key);
				}
				else
				{
					key.UpdateArrow();
				}
			}
			foreach (FromEntityIntentionBase item in toRemoveBuffer)
			{
				EraseArrow(item, activeArrows);
			}
		}
	}

	public void SetWorkflowArrows(Arrows workflowArrows)
	{
		_exclusiveWorkflowArrows = workflowArrows.Exclusive;
		_suppressedLineData.Clear();
		foreach (Arrows.LineData suppressedLine in workflowArrows.SuppressedLines)
		{
			_suppressedLineData.Add(suppressedLine);
		}
		EraseAllArrows(_activeArrowsWorkflow);
		if (workflowArrows == null)
		{
			SimpleLog.LogError("IntentionLineManager.SetWorkflowArrows - visualData is null!");
			return;
		}
		if (workflowArrows.LineDatas == null)
		{
			SimpleLog.LogError("IntentionLineManager.SetWorkflowArrows - visualData.LineDatas is null!");
			return;
		}
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		if (mtgGameState == null)
		{
			SimpleLog.LogError("IntentionLineManager.SetWorkflowArrows - currentGameState is null!");
			return;
		}
		foreach (Arrows.LineData lineData in workflowArrows.LineDatas)
		{
			MtgCardInstance cardById = mtgGameState.GetCardById(lineData.SourceEntityId);
			if (cardById == null)
			{
				continue;
			}
			if (!_entityViewProvider.TryGetCardView(lineData.SourceEntityId, out var cardView))
			{
				SimpleLog.LogError("IntentionLineManager.SetWorkflowArrows - sourceCardView is null!");
				continue;
			}
			if (cardById.Zone == null)
			{
				SimpleLog.LogError("IntentionLineManager.SetWorkflowArrows - sourceCard.Zone is null!");
				continue;
			}
			MtgEntity entityById = mtgGameState.GetEntityById(lineData.TargetEntityId);
			IEntityView entityView;
			if (entityById == null)
			{
				SimpleLog.LogError("IntentionLineManager.SetWorkflowArrows - targetEntity is null!");
			}
			else if (!_entityViewProvider.TryGetEntity(lineData.TargetEntityId, out entityView))
			{
				SimpleLog.LogError("IntentionLineManager.SetWorkflowArrows - targetEntityView is null!");
			}
			else if (cardById.Zone.Type == ZoneType.Stack)
			{
				DrawTargetingArrow(cardById, cardView, entityById, entityView, lineData.Group, lineData.GroupCount, _activeArrowsWorkflow);
			}
			else if (entityById is MtgCardInstance)
			{
				DrawArrow(GetPooledMediator<ToEntityFromEntityIntention>().Init(cardView, entityView, _entityViewProvider), _activeArrowsWorkflow, "Combat");
			}
			else
			{
				DrawArrow(GetPooledMediator<ToEntityFromEntityIntention>().Init(cardView, entityView, _entityViewProvider), _activeArrowsWorkflow, "Default");
			}
		}
		foreach (Arrows.LineData item in workflowArrows.CardsToMouse)
		{
			if (!mtgGameState.TryGetCard(item.SourceEntityId, out var card))
			{
				SimpleLog.LogError("IntentionLineManager.SetWorkflowArrows - visualData.CardsToMouse --> sourceCard is null!");
				return;
			}
			if (!_entityViewProvider.TryGetCardView(item.SourceEntityId, out var cardView2))
			{
				SimpleLog.LogError("IntentionLineManager.SetWorkflowArrows - visualData.CardsToMouse --> sourceCardView is null!");
				return;
			}
			if (card.Zone == null)
			{
				SimpleLog.LogError("IntentionLineManager.SetWorkflowArrows - visualData.CardsToMouse --> sourceCard.Zone is null!");
				return;
			}
			if (card.Zone.Type == ZoneType.Stack)
			{
				DrawArrow(GetPooledMediator<ToMouseFromSpellStackIntention>().Init(cardView2, item.Group, item.GroupCount), _activeArrowsWorkflow, "Target", (int)item.Group);
			}
			else if (_battlefield != null && (card.Zone.Type != ZoneType.Battlefield || _battlefield.CardIsStackParent(cardView2)))
			{
				DrawArrow(GetPooledMediator<ToMouseFromEntityIntention>().Init(cardView2), _activeArrowsWorkflow, "Target", (int)item.Group);
			}
		}
		bool flag = workflowArrows.CardsToMouse.Count > 0;
		if (!_prevWorkflowArrowsHadCardToMouse && flag)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_combat_target_draw_start, AudioManager.Default);
		}
		else if (_prevWorkflowArrowsHadCardToMouse && !flag)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_combat_target_draw_stop, AudioManager.Default);
		}
		_prevWorkflowArrowsHadCardToMouse = flag;
		UpdateArrowsVisibility();
	}

	private void OnVisibleGameStateChanged(MtgGameState gameState)
	{
		if (gameState != null)
		{
			EvaluateGameState(gameState);
			UpdateArrowsVisibility();
		}
	}

	private void OnUxEventCommenced(UXEvent uxEvent)
	{
		if (uxEvent is GameStatePlaybackCommencedUXEvent gameStatePlaybackCommencedUXEvent)
		{
			OnVisibleGameStateChanged(gameStatePlaybackCommencedUXEvent.GameState);
		}
		else if (uxEvent is GameStatePlaybackCompletedUXEvent gameStatePlaybackCompletedUXEvent)
		{
			OnVisibleGameStateChanged(gameStatePlaybackCompletedUXEvent.GameState);
		}
		else if (uxEvent is CombatFrame)
		{
			_inCombatUxEvent = true;
			UpdateArrowsVisibility();
		}
	}

	private void OnUxEventCompleted(UXEvent uxEvent)
	{
		if (uxEvent is CombatFrame)
		{
			_inCombatUxEvent = false;
			UpdateArrowsVisibility();
		}
	}

	private void OnBrowserShown(BrowserBase browser)
	{
		if (_shownBrowser == null)
		{
			_shownBrowser = browser;
			UpdateArrowsVisibility();
		}
	}

	private void OnBrowserHidden(BrowserBase browser)
	{
		if (_shownBrowser == browser)
		{
			_shownBrowser = null;
			UpdateArrowsVisibility();
		}
	}

	private void OnHoveredCardChanged(DuelScene_CDC cardView)
	{
		if (!(_hoveredCardView == cardView))
		{
			_hoveredCardView = cardView;
			MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
			if (mtgGameState != null)
			{
				EvaluateGameState(mtgGameState);
				UpdateArrowsVisibility();
			}
		}
	}

	private void OnHoveredAvatarChanged(IAvatarView avatarView)
	{
		if (_hoveredAvatarView != avatarView)
		{
			_hoveredAvatarView = avatarView;
			MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
			if (mtgGameState != null)
			{
				EvaluateGameState(mtgGameState);
				UpdateArrowsVisibility();
			}
		}
	}

	private void OnBattlefieldLaidOut()
	{
		MtgGameState mtgGameState = _gameStateProvider.CurrentGameState;
		if (mtgGameState != null)
		{
			EvaluateGameState(mtgGameState);
			UpdateArrowsVisibility();
		}
	}

	private void EvaluateGameState(MtgGameState gameState)
	{
		EraseAllArrows(_activeArrowsGameState);
		DrawGameStateStackArrows(gameState);
		DrawGameStateCombatArrows(gameState);
		_workflowProvider.GetCurrentWorkflow()?.UpdateArrowsPublic();
	}

	private void DrawGameStateStackArrows(MtgGameState gameState)
	{
		DuelScene_CDC duelScene_CDC = ((_hoveredCardView != null && _hoveredCardView.Model != null && _hoveredCardView.Model.ZoneType != ZoneType.Hand) ? _hoveredCardView : ((_stack != null) ? _stack.GetTopCardOnStack() : null));
		if (duelScene_CDC == null || _combatAnimationPlayer.SourceTransformsInCombat.Contains(duelScene_CDC.Root))
		{
			return;
		}
		MtgCardInstance cardById = gameState.GetCardById(duelScene_CDC.InstanceId);
		if (cardById == null)
		{
			return;
		}
		List<TargetSpec> list = _objectPool.PopObject<List<TargetSpec>>();
		List<TargetSpec> list2 = _objectPool.PopObject<List<TargetSpec>>();
		GetTargetSpecsInvolvingId(cardById.InstanceId, gameState.TargetInfo, list, list2);
		for (int i = 0; i < list.Count; i++)
		{
			foreach (uint item in list[i].Affected)
			{
				if (gameState.TryGetEntity(item, out var mtgEntity) && _entityViewProvider.TryGetEntity(item, out var entityView))
				{
					DrawTargetingArrow(cardById, duelScene_CDC, mtgEntity, entityView, (uint)i, (uint)list.Count, _activeArrowsGameState);
				}
			}
		}
		foreach (TargetSpec item2 in list2)
		{
			if (!gameState.TryGetEntity(item2.Affector, out var mtgEntity2) || !_entityViewProvider.TryGetEntity(item2.Affector, out var entityView2))
			{
				continue;
			}
			List<TargetSpec> list3 = _objectPool.PopObject<List<TargetSpec>>();
			GetTargetSpecsInvolvingId(item2.Affector, gameState.TargetInfo, list3);
			for (int j = 0; j < list3.Count; j++)
			{
				if (list3[j].Affected.Contains(cardById.InstanceId))
				{
					DrawTargetingArrow(mtgEntity2, entityView2, cardById, duelScene_CDC, (uint)j, (uint)list3.Count, _activeArrowsGameState);
				}
			}
			list3.Clear();
			_objectPool.PushObject(list3, tryClear: false);
		}
		list.Clear();
		list2.Clear();
		_objectPool.PushObject(list, tryClear: false);
		_objectPool.PushObject(list2, tryClear: false);
		foreach (MtgCardInstance child in cardById.Children)
		{
			if (child.ObjectType == GameObjectType.Ability && child.Parent == cardById && _entityViewProvider.TryGetEntity(child.InstanceId, out var entityView3))
			{
				DrawArrow(GetPooledMediator<ToSpellStackFromEntityIntention>().Init(duelScene_CDC, entityView3, _entityViewProvider), _activeArrowsGameState, "Ability");
			}
		}
		if (cardById.ObjectType == GameObjectType.Ability && cardById.Parent != null && _entityViewProvider.TryGetEntity(cardById.ParentId, out var entityView4))
		{
			DrawArrow(GetPooledMediator<ToSpellStackFromEntityIntention>().Init(entityView4, duelScene_CDC, _entityViewProvider), _activeArrowsGameState, "Ability");
		}
		HashSet<ReferenceMap.Reference> results = _objectPool.PopObject<HashSet<ReferenceMap.Reference>>();
		if (_referenceMap.GetTriggered(cardById.InstanceId, ref results))
		{
			foreach (ReferenceMap.Reference item3 in results)
			{
				uint b = item3.B;
				if (_entityViewProvider.TryGetEntity(b, out var entityView5))
				{
					DrawArrow(GetPooledMediator<ToSpellStackFromEntityIntention>().Init(duelScene_CDC, entityView5, _entityViewProvider), _activeArrowsGameState, "Ability");
				}
			}
		}
		results.Clear();
		_objectPool.PushObject(results, tryClear: false);
		HashSet<ReferenceMap.Reference> results2 = _objectPool.PopObject<HashSet<ReferenceMap.Reference>>();
		if (cardById.Parent != null && _referenceMap.GetTriggeredBy(cardById.InstanceId, ref results2))
		{
			foreach (ReferenceMap.Reference item4 in results2)
			{
				uint a = item4.A;
				if (cardById.Parent.InstanceId != a && _entityViewProvider.TryGetEntity(a, out var entityView6))
				{
					DrawArrow(GetPooledMediator<ToSpellStackFromEntityIntention>().Init(entityView6, duelScene_CDC, _entityViewProvider), _activeArrowsGameState, "Ability");
				}
			}
		}
		results2.Clear();
		_objectPool.PushObject(results2, tryClear: false);
		HashSet<ReferenceMap.Reference> results3 = _objectPool.PopObject<HashSet<ReferenceMap.Reference>>();
		if (_referenceMap.GetReferences(0u, ReferenceMap.ReferenceType.LinkedDamage, cardById.InstanceId, ref results3))
		{
			foreach (ReferenceMap.Reference item5 in results3)
			{
				uint a2 = item5.A;
				if (_entityViewProvider.TryGetEntity(a2, out var entityView7))
				{
					DrawArrow(GetPooledMediator<ToEntityFromEntityIntention>().Init(entityView7, duelScene_CDC, _entityViewProvider), _activeArrowsGameState, "Ability");
				}
			}
		}
		results3.Clear();
		_objectPool.PushObject(results3, tryClear: false);
		HashSet<ReferenceMap.Reference> results4 = _objectPool.PopObject<HashSet<ReferenceMap.Reference>>();
		if (_referenceMap.GetReferences(cardById.InstanceId, ReferenceMap.ReferenceType.LinkedDamage, 0u, ref results4))
		{
			foreach (ReferenceMap.Reference item6 in results4)
			{
				uint b2 = item6.B;
				if (_entityViewProvider.TryGetEntity(b2, out var entityView8))
				{
					DrawArrow(GetPooledMediator<ToEntityFromEntityIntention>().Init(duelScene_CDC, entityView8, _entityViewProvider), _activeArrowsGameState, "Ability");
				}
			}
		}
		results4.Clear();
		_objectPool.PushObject(results4, tryClear: false);
		if (duelScene_CDC.HolderType != CardHolderType.Command)
		{
			return;
		}
		HashSet<ReferenceMap.Reference> results5 = _objectPool.PopObject<HashSet<ReferenceMap.Reference>>();
		if (_referenceMap.GetReferences(cardById.InstanceId, ReferenceMap.ReferenceType.LinkedTo, 0u, ref results5))
		{
			foreach (ReferenceMap.Reference item7 in results5)
			{
				uint b3 = item7.B;
				if (_entityViewProvider.TryGetEntity(b3, out var entityView9) && entityView9 is DuelScene_CDC { HolderType: CardHolderType.Battlefield })
				{
					DrawArrow(GetPooledMediator<ToEntityFromSpellStackIntention>().Init(duelScene_CDC, entityView9, _entityViewProvider), _activeArrowsGameState, "Ability");
				}
			}
		}
		results5.Clear();
		_objectPool.PushObject(results5, tryClear: false);
	}

	private void DrawGameStateCombatArrows(MtgGameState gameState)
	{
		if (_battlefield == null || (gameState.CurrentStep != Step.DeclareAttack && gameState.CurrentStep != Step.DeclareBlock && gameState.CurrentStep != Step.FirstStrikeDamage && gameState.CurrentStep != Step.CombatDamage))
		{
			return;
		}
		DuelScene_CDC hoveredCardView = _hoveredCardView;
		MtgCardInstance mtgCardInstance = ((hoveredCardView != null && hoveredCardView.Model != null) ? gameState.GetCardById(hoveredCardView.InstanceId) : null);
		IAvatarView hoveredAvatarView = _hoveredAvatarView;
		MtgPlayer mtgPlayer = hoveredAvatarView?.Model;
		_attackingCards.Clear();
		_unblockedAttackers.Clear();
		_blockingCards.Clear();
		_attackQuarries.Clear();
		_activeCombatArrows.Clear();
		foreach (KeyValuePair<uint, MtgCardInstance> visibleCard in gameState.VisibleCards)
		{
			MtgCardInstance value = visibleCard.Value;
			if (value.IsAttacking)
			{
				_attackingCards.Add(value);
				_attackQuarries.Add(value.AttackTargetId);
				if (value.BlockedByIds.Count == 0)
				{
					_unblockedAttackers.Add(value);
				}
			}
			else if (value.IsBlocking)
			{
				_blockingCards.Add(value);
			}
		}
		IEntityView entityView2;
		if (mtgPlayer == null)
		{
			IEntityView entityView = hoveredCardView;
			entityView2 = entityView;
		}
		else
		{
			IEntityView entityView = hoveredAvatarView;
			entityView2 = entityView;
		}
		IEntityView entityView3 = entityView2;
		uint num = entityView3?.InstanceId ?? 0;
		if (_attackQuarries.Contains(num))
		{
			DrawHoveredQuarryArrows(_attackingCards, num, entityView3);
			return;
		}
		if (_blockingCards.Contains(mtgCardInstance))
		{
			DrawHoveredBlockerArrows(mtgCardInstance);
			return;
		}
		if (_attackingCards.Contains(mtgCardInstance))
		{
			DrawHoveredAttackerArrows(mtgCardInstance);
			return;
		}
		if (gameState.IsMultiplayer)
		{
			foreach (MtgCardInstance attackingCard in _attackingCards)
			{
				DrawHoveredAttackerArrows(attackingCard);
			}
			return;
		}
		foreach (MtgCardInstance attackingCard2 in _attackingCards)
		{
			if (_entityViewProvider.TryGetCardView(attackingCard2.InstanceId, out var cardView) && _battlefield.CardIsStackParent(cardView))
			{
				DrawAttackerArrow(attackingCard2, cardView);
			}
		}
	}

	private void DrawHoveredQuarryArrows(IEnumerable<MtgCardInstance> attackingCards, uint entityId, IEntityView entityView)
	{
		foreach (MtgCardInstance attackingCard in attackingCards)
		{
			if (attackingCard.AttackTargetId == entityId && _unblockedAttackers.Contains(attackingCard) && _entityViewProvider.TryGetCardView(attackingCard.InstanceId, out var cardView) && _battlefield.CardIsStackParent(cardView))
			{
				DrawArrow(GetPooledMediator<ToEntityFromEntityIntention>().Init(cardView, entityView, _entityViewProvider), _activeArrowsGameState, "Link");
			}
		}
	}

	private void DrawHoveredBlockerArrows(MtgCardInstance card)
	{
		if (!_entityViewProvider.TryGetCardView(card.InstanceId, out var cardView))
		{
			return;
		}
		foreach (uint blockingId in card.BlockingIds)
		{
			if (_entityViewProvider.TryGetCardView(blockingId, out var cardView2))
			{
				DrawCombatArrow(GetStackParent(cardView2), cardView, _activeArrowsGameState);
			}
		}
	}

	private void DrawHoveredAttackerArrows(MtgCardInstance card)
	{
		if (!_entityViewProvider.TryGetCardView(card.InstanceId, out var cardView))
		{
			return;
		}
		if (_unblockedAttackers.Contains(card))
		{
			DuelScene_CDC cardView2;
			if (_entityViewProvider.TryGetAvatarById(card.AttackTargetId, out var avatar))
			{
				DrawArrow(GetPooledMediator<ToEntityFromEntityIntention>().Init(cardView, avatar, _entityViewProvider), _activeArrowsGameState, "Link");
			}
			else if (_entityViewProvider.TryGetCardView(card.AttackTargetId, out cardView2))
			{
				DrawArrow(GetPooledMediator<ToEntityFromEntityIntention>().Init(cardView, cardView2, _entityViewProvider), _activeArrowsGameState, "Link");
			}
			return;
		}
		foreach (uint blockedById in card.BlockedByIds)
		{
			if (_entityViewProvider.TryGetCardView(blockedById, out var cardView3))
			{
				cardView3 = GetStackParent(cardView3);
				if (!(cardView3 == null) && (cardView3.Model.Instance.BlockState == BlockState.Declared || cardView3.Model.Instance.BlockState == BlockState.Blocking))
				{
					DrawCombatArrow(cardView, cardView3, _activeArrowsGameState);
				}
			}
		}
	}

	private void DrawAttackerArrow(MtgCardInstance attackingCard, DuelScene_CDC attackingCardView)
	{
		if (_unblockedAttackers.Contains(attackingCard))
		{
			if (_entityViewProvider.TryGetCardView(attackingCard.AttackTargetId, out var cardView))
			{
				DrawArrow(GetPooledMediator<ToEntityFromEntityIntention>().Init(attackingCardView, cardView, _entityViewProvider), _activeArrowsGameState, "Link");
			}
			return;
		}
		foreach (uint blockedById in attackingCard.BlockedByIds)
		{
			if (_entityViewProvider.TryGetCardView(blockedById, out var cardView2))
			{
				cardView2 = GetStackParent(cardView2);
				if (!(cardView2 == null) && (cardView2.Model.Instance.BlockState == BlockState.Declared || cardView2.Model.Instance.BlockState == BlockState.Blocking))
				{
					DrawCombatArrow(attackingCardView, cardView2, _activeArrowsGameState);
				}
			}
		}
	}

	private void DrawTargetingArrow(MtgEntity sourceEntity, IEntityView sourceEntityView, MtgEntity targetEntity, IEntityView targetEntityView, uint group, uint groupCount, Dictionary<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> activeArrows)
	{
		if (!(sourceEntityView is DuelScene_CDC duelScene_CDC))
		{
			return;
		}
		if (duelScene_CDC.CurrentCardHolder.CardHolderType == CardHolderType.Stack)
		{
			if (targetEntityView is DuelScene_CDC duelScene_CDC2)
			{
				if (duelScene_CDC2.CurrentCardHolder.CardHolderType == CardHolderType.Stack)
				{
					DrawArrow(GetPooledMediator<ToSpellStackFromSpellStackIntention>().Init(sourceEntityView, targetEntityView, _entityViewProvider, group, groupCount), activeArrows, "Target", (int)group);
				}
				else if (duelScene_CDC2.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield)
				{
					if (_battlefield == null)
					{
						return;
					}
					IBattlefieldStack stackForCard = _battlefield.GetStackForCard(duelScene_CDC2);
					if (stackForCard == null)
					{
						return;
					}
					if (stackForCard.StackParent == duelScene_CDC2)
					{
						if (IsDistributionTargetArrow(_gameStateProvider.CurrentGameState, sourceEntity, targetEntity))
						{
							DrawArrow(GetPooledMediator<DistributionToEntityFromSpellStackIntention>().Init(sourceEntityView, targetEntityView, _entityViewProvider, group, groupCount), activeArrows, "Target", (int)group);
						}
						else
						{
							DrawArrow(GetPooledMediator<ToEntityFromSpellStackIntention>().Init(sourceEntityView, targetEntityView, _entityViewProvider, group, groupCount), activeArrows, "Target", (int)group);
						}
					}
					else
					{
						DrawArrow(GetPooledMediator<ToBattlefieldStackFromSpellStackIntention>().Init(sourceEntityView, targetEntityView, _battlefield, _entityViewProvider, group, groupCount), activeArrows, "Target", (int)group);
					}
				}
				else
				{
					DrawArrow(GetPooledMediator<ToEntityFromSpellStackIntention>().Init(sourceEntityView, targetEntityView, _entityViewProvider, group, groupCount), activeArrows, "Target", (int)group);
				}
			}
			else if (targetEntityView is DuelScene_AvatarView)
			{
				if (IsDistributionTargetArrow(_gameStateProvider.CurrentGameState, sourceEntity, targetEntity))
				{
					DrawArrow(GetPooledMediator<DistributionToEntityFromSpellStackIntention>().Init(sourceEntityView, targetEntityView, _entityViewProvider, group, groupCount), activeArrows, "Target", (int)group);
				}
				else
				{
					DrawArrow(GetPooledMediator<ToEntityFromSpellStackIntention>().Init(sourceEntityView, targetEntityView, _entityViewProvider, group, groupCount), activeArrows, "Target", (int)group);
				}
			}
		}
		else if (duelScene_CDC.CurrentCardHolder.CardHolderType == CardHolderType.Battlefield || (duelScene_CDC.CurrentCardHolder.CardHolderType == CardHolderType.Command && duelScene_CDC.Model.GrpId == 165968))
		{
			DrawArrow(GetPooledMediator<ToEntityFromEntityIntention>().Init(sourceEntityView, targetEntityView, _entityViewProvider), activeArrows, "Target", (int)group);
		}
	}

	private void DrawCombatArrow(IEntityView attackingCardView, IEntityView blockingCardView, IDictionary<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> activeArrows)
	{
		if (attackingCardView != null && _activeCombatArrows.Add((attackingCardView.InstanceId, blockingCardView.InstanceId)))
		{
			DrawArrow(GetPooledMediator<ToEntityFromEntityIntention>().Init(attackingCardView, blockingCardView, _entityViewProvider), activeArrows, "Combat");
		}
	}

	private void DrawArrow(FromEntityIntentionBase mediator, IDictionary<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> activeArrows, string prefabName, int? targetIndex = null)
	{
		if (!activeArrows.ContainsKey(mediator))
		{
			DreamteckIntentionArrowBehavior dreamteckIntentionArrowBehavior = IntentionLineUtils.CreateIntentionLine(_assetLookupSystem, prefabName, _unityPool, targetIndex);
			if (!(dreamteckIntentionArrowBehavior == null))
			{
				GameObject gameObject = dreamteckIntentionArrowBehavior.gameObject;
				gameObject.UpdateActive(mediator is FromSpellStackIntentionBase);
				gameObject.transform.SetParent(_arrowRoot);
				mediator.ArrowBehavior = dreamteckIntentionArrowBehavior;
				activeArrows.Add(mediator, dreamteckIntentionArrowBehavior);
			}
		}
	}

	private void EraseArrow(FromEntityIntentionBase mediator, Dictionary<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> activeArrows)
	{
		if (activeArrows.TryGetValue(mediator, out var value))
		{
			value.gameObject.UpdateActive(active: false);
			activeArrows.Remove(mediator);
			if (!_mediatorPool.TryGetValue(mediator.GetType(), out var value2))
			{
				value2 = new Queue<FromEntityIntentionBase>();
				_mediatorPool.Add(mediator.GetType(), value2);
			}
			value2.Enqueue(mediator);
			mediator.OnPooled();
			_unityPool.PushObject(value.gameObject);
		}
	}

	private void EraseAllArrows(Dictionary<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> activeArrows)
	{
		_toRemoveBuffer.Clear();
		_toRemoveBuffer.AddRange(activeArrows.Keys);
		foreach (FromEntityIntentionBase item in _toRemoveBuffer)
		{
			EraseArrow(item, activeArrows);
		}
	}

	private void UpdateArrowsVisibility()
	{
		foreach (KeyValuePair<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> item in _activeArrowsGameState)
		{
			FromEntityIntentionBase key = item.Key;
			key.SetArrowVisible(GameStateArrowIsVisible(key));
		}
		foreach (KeyValuePair<FromEntityIntentionBase, DreamteckIntentionArrowBehavior> item2 in _activeArrowsWorkflow)
		{
			item2.Key.SetArrowVisible(_shownBrowser == null && !_inCombatUxEvent);
		}
	}

	private bool GameStateArrowIsVisible(FromEntityIntentionBase arrow)
	{
		if (_shownBrowser != null)
		{
			return false;
		}
		if (_inCombatUxEvent)
		{
			return false;
		}
		if (_exclusiveWorkflowArrows && _hoveredCardView == null)
		{
			return false;
		}
		foreach (Arrows.LineData suppressedLineDatum in _suppressedLineData)
		{
			if (suppressedLineDatum.SourceEntityId == arrow.SourceEntityId && suppressedLineDatum.TargetEntityId == getTargetArrowId(arrow))
			{
				return false;
			}
		}
		return true;
		static uint getTargetArrowId(FromEntityIntentionBase fromEntityIntentionBase)
		{
			if (fromEntityIntentionBase is ToEntityFromEntityIntention { EndEntityId: var endEntityId })
			{
				return endEntityId;
			}
			if (!(fromEntityIntentionBase is ToEntityFromSpellStackIntention { EndEntityId: var endEntityId2 }))
			{
				return 0u;
			}
			return endEntityId2;
		}
	}

	private bool IsDistributionTargetArrow(MtgGameState gameState, MtgEntity sourceEntity, MtgEntity targetEntity)
	{
		bool result = false;
		foreach (TargetSpec item in gameState.TargetInfo)
		{
			if (item.Affector != sourceEntity.InstanceId)
			{
				continue;
			}
			if (item.Distributions.Count > 0)
			{
				if (item.Affected.Contains(targetEntity.InstanceId))
				{
					result = true;
					break;
				}
			}
			else if (_workflowProvider.GetPendingWorkflow() is DistributionWorkflow || _workflowProvider.GetCurrentWorkflow() is DistributionWorkflow)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	private DuelScene_CDC GetStackParent(DuelScene_CDC cdc)
	{
		if (_battlefield == null)
		{
			return null;
		}
		if (_battlefield.CardIsStackParent(cdc))
		{
			return cdc;
		}
		IBattlefieldStack stackForCard = _battlefield.GetStackForCard(cdc);
		if (stackForCard == null || stackForCard.StackParent == null)
		{
			return null;
		}
		return stackForCard.StackParent;
	}

	private T GetPooledMediator<T>() where T : FromEntityIntentionBase, new()
	{
		if (_mediatorPool.TryGetValue(typeof(T), out var value) && value.Count > 0)
		{
			return value.Dequeue() as T;
		}
		return new T();
	}

	private void GetTargetSpecsInvolvingId(uint entityId, IEnumerable<TargetSpec> targetSpecs, ICollection<TargetSpec> outAffectorTargetSpecs = null, ICollection<TargetSpec> outAffectedTargetSpecs = null)
	{
		outAffectorTargetSpecs?.Clear();
		outAffectedTargetSpecs?.Clear();
		foreach (TargetSpec targetSpec in targetSpecs)
		{
			if (targetSpec.Affector == entityId)
			{
				outAffectorTargetSpecs?.Add(targetSpec);
			}
			if (targetSpec.Affected.Contains(entityId))
			{
				outAffectedTargetSpecs?.Add(targetSpec);
			}
		}
	}

	public void Dispose()
	{
		CardHoverController.HoveredAvatarChangedHandlers -= OnHoveredAvatarChanged;
		CardHoverController.OnHoveredCardUpdated -= OnHoveredCardChanged;
		_cardHolderCreatedEvent.Listeners -= OnCardHolderCreated;
		if (_browserManager != null)
		{
			_browserManager.BrowserHidden -= OnBrowserHidden;
			_browserManager.BrowserShown -= OnBrowserShown;
		}
		_uxEventQueue.EventExecutionCommenced -= OnUxEventCommenced;
		_uxEventQueue.EventExecutionCompleted -= OnUxEventCompleted;
		EraseAllArrows(_activeArrowsGameState);
		EraseAllArrows(_activeArrowsWorkflow);
		foreach (Type key in _mediatorPool.Keys)
		{
			if (_mediatorPool.TryGetValue(key, out var value))
			{
				while (value.Count > 0)
				{
					value.Dequeue()?.Dispose();
				}
			}
		}
		_attackingCards.Clear();
		_objectPool.PushObject(_attackingCards, tryClear: false);
		_unblockedAttackers.Clear();
		_objectPool.PushObject(_unblockedAttackers, tryClear: false);
		_blockingCards.Clear();
		_objectPool.PushObject(_blockingCards, tryClear: false);
		_attackQuarries.Clear();
		_objectPool.PushObject(_attackQuarries, tryClear: false);
		_activeCombatArrows.Clear();
		_objectPool.PushObject(_activeCombatArrows, tryClear: false);
		_toRemoveBuffer.Clear();
		_objectPool.PushObject(_toRemoveBuffer, tryClear: false);
		_mediatorPool.Clear();
		_objectPool.PushObject(_mediatorPool, tryClear: false);
		_activeArrowsGameState.Clear();
		_objectPool.PushObject(_activeArrowsGameState, tryClear: false);
		_activeArrowsWorkflow.Clear();
		_objectPool.PushObject(_activeArrowsWorkflow, tryClear: false);
		_suppressedLineData.Clear();
		_objectPool.PushObject(_suppressedLineData, tryClear: false);
		if (_battlefield != null)
		{
			_battlefield.OnCardHolderUpdated -= OnBattlefieldLaidOut;
			_battlefield = null;
		}
		_stack = null;
	}
}
