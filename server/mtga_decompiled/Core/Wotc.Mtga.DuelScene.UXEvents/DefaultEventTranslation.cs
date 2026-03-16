using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using GreClient.CardData;
using GreClient.Rules;
using Pooling;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.DuelScene.Companions;
using Wotc.Mtga.DuelScene.Emotes;
using Wotc.Mtga.DuelScene.Input;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.DuelScene.ZoneCounts;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class DefaultEventTranslation : IEventTranslation, IDisposable
{
	private readonly IContext _context;

	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly GameManager _gameManager;

	private readonly IGameStatePlaybackController _gameStatePlaybackController;

	private readonly Dictionary<Type, IEventTranslator> _eventTranslators;

	private readonly IUXEventGrouper _resolutionEventPostProcess;

	private readonly IUXEventGrouper _combatFramePostProcess;

	private readonly IUXEventGrouper _postProcess;

	public DefaultEventTranslation(GameManager gameManager, AssetLookupSystem assetLookupSystem, IContext context, IUXEventGrouper postProcess)
	{
		_context = context;
		_assetLookupSystem = assetLookupSystem;
		_gameManager = gameManager;
		_gameStatePlaybackController = context.Get<IGameStatePlaybackController>() ?? NullGameStatePlaybackController.Default;
		_eventTranslators = new Dictionary<Type, IEventTranslator>
		{
			{
				typeof(ShuffleEvent),
				new ShuffleEventTranslator(context.Get<ICardDatabaseAdapter>(), context.Get<IGameStateProvider>(), context.Get<ICardViewManager>(), context.Get<ICardHolderProvider>())
			},
			{
				typeof(AbilityAddedEvent),
				new AbilityAddedEventTranslator(context.Get<IEntityViewProvider>(), context.Get<IAbilityDataProvider>(), context.Get<IPlayerAbilityController>())
			},
			{
				typeof(AbilityRemovedEvent),
				new AbilityRemovedEventTranslator(context.Get<IEntityViewProvider>(), context.Get<IPlayerAbilityController>())
			},
			{
				typeof(CardChangedEvent),
				new CardChangedEventTranslator(gameManager)
			},
			{
				typeof(RevealedCardChangedEvent),
				new RevealedCardChangedEventTranslator(context.Get<IRevealedCardsController>())
			},
			{
				typeof(CardDecoratorUpdatedEvent),
				new CardDecoratorUpdatedEventTranslator(gameManager)
			},
			{
				typeof(CardCreatedEvent),
				new CardCreatedEventTranslator(context, assetLookupSystem, gameManager)
			},
			{
				typeof(ChoosingAttachmentsEvent),
				new ChoosingAttachmentsEventTranslator(context.Get<IGameStateProvider>(), context.Get<ICardHolderProvider>())
			},
			{
				typeof(DieRollEvent),
				new DieRollEventTranslator(context.Get<ICardDatabaseAdapter>(), context.Get<IVfxProvider>(), assetLookupSystem)
			},
			{
				typeof(MultistepEffectEvent),
				new MultistepEffectEventTranslator(gameManager)
			},
			{
				typeof(PlayerSelectingTargetsEvent),
				new PlayerSelectingTargetsEventTranslator(assetLookupSystem, context.Get<IVfxProvider>(), context.Get<ICardDatabaseAdapter>())
			},
			{
				typeof(PlayerSubmittedTargetsEvent),
				new PlayerSubmittedTargetsEventTranslator(assetLookupSystem, context.Get<IVfxProvider>(), context.Get<ICardDatabaseAdapter>())
			},
			{
				typeof(DelayedTriggerCreated),
				new AddDelayedTriggerTranslator(context.Get<IDelayedTriggerController>())
			},
			{
				typeof(DelayedTriggerUpdated),
				new UpdateDelayedTriggerTranslator(context.Get<IDelayedTriggerController>())
			},
			{
				typeof(DelayedTriggerDeleted),
				new RemoveDelayedTriggerTranslator(context.Get<IDelayedTriggerController>())
			},
			{
				typeof(DesignationDeletedEvent),
				new DesignationDeletedTranslator(context.Get<IDesignationController>(), gameManager)
			},
			{
				typeof(DesignationUpdatedEvent),
				new DesignationUpdatedTranslator(context.Get<IDesignationController>(), gameManager)
			},
			{
				typeof(DesignationCreatedEvent),
				new DesignationCreatedTranslator(context.Get<IDesignationController>(), gameManager)
			},
			{
				typeof(TokenImmediatelyDiedEvent),
				new TokenImmediatelyDiedEventTranslator()
			},
			{
				typeof(CardReactionEvent),
				new CardReactionEventTranslator(context.Get<ICardViewProvider>())
			},
			{
				typeof(SuspendLikeInfoCreated),
				new SuspendLikeInfoCreatedTranslator(context.Get<ISuspendLikeController>())
			},
			{
				typeof(SuspendLikeInfoUpdated),
				new SuspendLikeInfoUpdatedTranslator(context.Get<ISuspendLikeController>())
			},
			{
				typeof(SuspendLikeInfoRemoved),
				new SuspendLikeInfoRemovedTranslator(context.Get<ISuspendLikeController>())
			},
			{
				typeof(ExtraTurnsChanged),
				new ExtraTurnChangedTranslator(context.Get<ITurnController>(), context.Get<IExtraTurnRenderer>())
			},
			{
				typeof(ReplacementEffectChangedEvent),
				new ReplacementEffectChangedTranslator(gameManager, context.Get<IReplacementEffectController>())
			},
			{
				typeof(UpdateGamewideCountEvent),
				new UpdateGamewideCountEventTranslator(context.Get<IGamewideCountController>())
			},
			{
				typeof(AddedGamewideCountEvent),
				new AddedGamewideCountEventTranslator(context.Get<IGamewideCountController>())
			},
			{
				typeof(RemovedGamewideCountEvent),
				new RemovedGamewideCountEventTranslator(context.Get<IGamewideCountController>())
			},
			{
				typeof(UpdatePendingEffectEvent),
				new UpdatePendingEffectEventTranslator(context.Get<IPendingEffectController>())
			},
			{
				typeof(HypotheticalActionsChangedEvent),
				new HypotheticalActionsChangedEventTranslator(context)
			},
			{
				typeof(UpdateQualificationEvent),
				new UpdateQualificationEventTranslator(context.Get<IQualificationController>())
			},
			{
				typeof(CountersChangedEvent),
				new CountersChangedEventTranslator(gameManager)
			},
			{
				typeof(RegeneratedCardEvent),
				new RegeneratedCardEventTranslator(context.Get<ICardViewProvider>(), context.Get<IVfxProvider>(), assetLookupSystem)
			},
			{
				typeof(DamageDealtEvent),
				new DamageDealtEventTranslator(gameManager)
			},
			{
				typeof(TurnChangeEvent),
				new TurnChangeEventTranslator(context.Get<ITurnController>(), context.Get<IPlayerFocusController>(), context.Get<ICardHolderProvider>(), gameManager.UIManager)
			},
			{
				typeof(DecidingPlayerChangeEvent),
				new DecidingPlayerChangeEventTranslator(context.Get<IAvatarViewProvider>(), context.Get<ITurnController>(), context.Get<TimerManager>(), context.Get<IVfxProvider>(), assetLookupSystem)
			},
			{
				typeof(GamePhaseChangeEvent),
				new GamePhaseChangeEventTranslator(gameManager, context.Get<ITurnController>(), context.Get<ICardViewProvider>())
			},
			{
				typeof(DisqualifiedEffectEvent),
				new DisqualifiedEffectEventTranslator(gameManager, context)
			},
			{
				typeof(ResolutionEvent),
				new ResolutionEventTranslator(gameManager, assetLookupSystem, context)
			},
			{
				typeof(ManaPaidEvent),
				new ManaPaidEventTranslator(gameManager)
			},
			{
				typeof(UpdateManaPoolEvent),
				new UpdateManaPoolEventTranslator(context.Get<IAvatarViewProvider>(), context.Get<IObjectPool>(), gameManager)
			},
			{
				typeof(UpdatePlayerLifeTotal),
				new UpdatePlayerLifeTotalTranslator(context, gameManager.SplineCache, assetLookupSystem)
			},
			{
				typeof(ObjectSelectedEvent),
				new ObjectSelectedEventTranslator(context.Get<ICardViewProvider>(), context.Get<IVfxProvider>(), assetLookupSystem)
			},
			{
				typeof(ObjectDeselectedEvent),
				new ObjectDeselectedEventTranslator(context.Get<ICardViewProvider>())
			},
			{
				typeof(ReplacementEffectAppliedEvent),
				new ReplacementEffectAppliedEventTranslator(context.Get<IAbilityDataProvider>(), context.Get<ICardViewProvider>(), context.Get<IVfxProvider>(), assetLookupSystem)
			},
			{
				typeof(IllegalAttachmentEvent),
				new IllegalAttachmentEventTranslator(context.Get<IAbilityDataProvider>(), context.Get<ICardViewProvider>(), context.Get<IVfxProvider>(), assetLookupSystem)
			},
			{
				typeof(UpdatePlayerHandSize),
				new UpdatePlayerHandSizeTranslator(context.Get<ICardHolderProvider>())
			},
			{
				typeof(CreatePlayerEvent),
				new CreatePlayerEventTranslator(context.Get<IAvatarViewController>(), context.Get<IAvatarInputController>(), context.Get<IEmoteManager>(), context.Get<ICompanionViewController>(), context.Get<IZoneCountController>(), gameManager.UIManager.BattleFieldStaticElementsLayout)
			},
			{
				typeof(ChoiceResultEvent),
				new ChoiceResultEventTranslator(gameManager)
			},
			{
				typeof(ZoneUpdatedEvent),
				new ZoneUpdatedEventTranslator(context.Get<ICardHolderProvider>())
			},
			{
				typeof(CreateZoneEvent),
				new CreateZoneEventTranslator(context.Get<IGameStateProvider>(), context.Get<ICardHolderManager>())
			},
			{
				typeof(SyntheticEvent),
				new SyntheticEventTranslator(context, assetLookupSystem)
			},
			{
				typeof(UserActionTakenEvent),
				new UserActionTakenTranslator(context, assetLookupSystem)
			},
			{
				typeof(GameEndTranslator),
				new GameEndTranslator()
			},
			{
				typeof(CardNamedEvent),
				new CardNamedEventTranslator(context)
			},
			{
				typeof(DeletePlayerEvent),
				new DeletePlayerEventTranslator(context.Get<IGameStateProvider>(), context.Get<IAvatarViewController>(), context.Get<ICardHolderController>(), context.Get<ICardViewManager>())
			},
			{
				typeof(DeleteZoneEvent),
				new DeleteZoneEventTranslator(context.Get<ICardHolderController>())
			},
			{
				typeof(CoinFlipEvent),
				new CoinFlipEventTranslator(context.Get<IUnityObjectPool>(), assetLookupSystem)
			},
			{
				typeof(ScryResultEvent),
				new ScryResultEventTranslator(context, assetLookupSystem)
			},
			{
				typeof(PlayerControllerChanged),
				new PlayerControllerChangedEventTranslator(context.Get<IGameStateProvider>(), context.Get<IAvatarViewProvider>(), context.Get<IVfxProvider>(), assetLookupSystem)
			},
			{
				typeof(ZoneChangeEvent),
				new ZoneChangeEventTranslator(context, assetLookupSystem, gameManager)
			},
			{
				typeof(AddPlayerNumericAid),
				new AddPlayerNumericAidEventTranslator(context.Get<IPlayerNumericAidController>())
			},
			{
				typeof(UpdatePlayerNumericAid),
				new UpdatePlayerNumericAidEventTranslator(context.Get<IPlayerNumericAidController>())
			},
			{
				typeof(RemovePlayerNumericAid),
				new RemovePlayerNumericAidEventTranslator(context.Get<IPlayerNumericAidController>())
			}
		};
		if (gameManager.SessionType == GameSessionType.Game)
		{
			_eventTranslators[typeof(WaitingOnStartingPlayerEvent)] = new WaitingOnStartingPlayerEventTranslator(context, gameManager.UXEventQueue);
		}
		NPEDirector npeDirector = gameManager.NpeDirector;
		if (npeDirector != null)
		{
			_eventTranslators[typeof(AttackSelectionEvent)] = new AttackSelectionEventTranslator(context, assetLookupSystem, npeDirector);
		}
		_resolutionEventPostProcess = new ZeroIndexPostProcessAggregate(new ResolutionEventReordering(), new RemoveResolutionCoinFlips(), new OverrideResolutionEvents<UXEventDamageDealt>(assetLookupSystem, context.Get<IVfxProvider>()), new ResolutionEventCardViewImmediateUpdate(context.Get<IGameStateProvider>(), context.Get<ICardDatabaseAdapter>(), context.Get<ICardViewProvider>()));
		_combatFramePostProcess = new CombatFramePostProcess(gameManager, context.Get<ICardViewProvider>(), new ZoneTransferGrouper(context.Get<ICardHolderProvider>(), context.Get<IVfxProvider>(), assetLookupSystem, gameManager));
		_postProcess = postProcess ?? NullUXEventGrouper.Default;
	}

	public List<UXEvent> GenerateEvents(GreClient.Rules.GameStateUpdate gameStateUpdate)
	{
		List<GameRulesEvent> changes = gameStateUpdate.Changes;
		MtgGameState newState = gameStateUpdate.NewState;
		List<UXEvent> events = new List<UXEvent>();
		CheckForPendingReplacementEffectTokens(gameStateUpdate, events);
		for (int i = 0; i < changes.Count; i++)
		{
			Translate(changes, i, gameStateUpdate.OldState, gameStateUpdate.NewState, events);
		}
		HandleMergeBreakups(events, gameStateUpdate.NewState.Battlefield);
		ToxicOrInfectCounterProducedUXEvent(events);
		_resolutionEventPostProcess.GroupEvents(0, ref events);
		_combatFramePostProcess.GroupEvents(0, ref events);
		events.Insert(0, new GameStatePlaybackCommencedUXEvent(newState, _gameStatePlaybackController));
		_postProcess.GroupEvents(0, ref events);
		events.Add(new GameStatePlaybackCompletedUXEvent(newState, _gameStatePlaybackController));
		return events;
	}

	private void CheckForPendingReplacementEffectTokens(GreClient.Rules.GameStateUpdate gsUpdate, List<UXEvent> events)
	{
		BaseUserRequest interaction = gsUpdate.Interaction;
		MtgGameState newState = gsUpdate.NewState;
		if (!(interaction is SelectNRequest selectNRequest))
		{
			return;
		}
		ReplacementEffectData? replacementEffectData = null;
		if (newState.ReplacementEffects.TryGetValue(selectNRequest.SourceId, out var value))
		{
			foreach (ReplacementEffectData item2 in value)
			{
				if (item2.AffectorId == selectNRequest.SourceId)
				{
					replacementEffectData = item2;
					break;
				}
			}
		}
		if (replacementEffectData.HasValue)
		{
			uint affectedId = replacementEffectData.Value.AffectedId;
			if (newState.VisibleCards.TryGetValue(affectedId, out var value2) && value2.ObjectType == GameObjectType.Token)
			{
				ZoneTransferUXEvent item = new ZoneTransferUXEvent(_context, _assetLookupSystem, _gameManager, value2.InstanceId, null, value2.InstanceId, value2, null, null, newState.Battlefield, ZoneTransferReason.CardCreated);
				events.Add(item);
			}
		}
	}

	private static void HandleMergeBreakups(List<UXEvent> events, MtgZone battlefield)
	{
		for (int i = 0; i < events.Count; i++)
		{
			if (!(events[i] is ZoneTransferUXEvent { IsMergeBreakup: not false } zoneTransferUXEvent))
			{
				continue;
			}
			int num = 0;
			ZoneTransferUXEvent zoneTransferUXEvent2 = null;
			for (int j = i + 1; j < events.Count; j++)
			{
				if (zoneTransferUXEvent2 != null)
				{
					break;
				}
				if (!(events[j] is ZoneTransferUXEvent zoneTransferUXEvent3))
				{
					break;
				}
				if (zoneTransferUXEvent.OldId != zoneTransferUXEvent3.OldId)
				{
					break;
				}
				if (zoneTransferUXEvent3.IsMergeBreakup)
				{
					num++;
				}
				else
				{
					zoneTransferUXEvent2 = zoneTransferUXEvent3;
				}
			}
			if (zoneTransferUXEvent2 != null)
			{
				for (int k = i; k < i + 1 + num; k++)
				{
					if (events[k] is ZoneTransferUXEvent zoneTransferUXEvent4)
					{
						zoneTransferUXEvent4.Reason = zoneTransferUXEvent2.Reason;
					}
				}
			}
			else if (events[i + num] is ZoneTransferUXEvent zoneTransferUXEvent5)
			{
				events[i + num] = zoneTransferUXEvent5.Clone(zoneTransferUXEvent5.OldId, zoneTransferUXEvent5.OldInstance, zoneTransferUXEvent5.NewId, zoneTransferUXEvent5.NewInstance, zoneTransferUXEvent5.Instigator, battlefield, zoneTransferUXEvent5.ToZone, zoneTransferUXEvent5.Reason);
			}
			i += num;
		}
	}

	private void Translate(List<GameRulesEvent> changes, int index, MtgGameState oldState, MtgGameState newState, List<UXEvent> events)
	{
		if (_eventTranslators.TryGetValue(changes[index].GetType(), out var value))
		{
			value.Translate(changes, index, oldState, newState, events);
			return;
		}
		GameRulesEvent gameRulesEvent = changes[index];
		if (!(gameRulesEvent is InstanceIdChange iic))
		{
			if (!(gameRulesEvent is CardDeletedEvent cde))
			{
				if (gameRulesEvent is CardRevealedEvent cardRevealedEvent)
				{
					CardRevealedEvent_EvtTranslate(changes, index, events, cardRevealedEvent);
				}
			}
			else
			{
				CardDeletedEvent_EvtTranslate(oldState, events, cde);
			}
		}
		else
		{
			InstanceIdChangeTranslate(_context, _assetLookupSystem, _gameManager, changes, index, oldState, newState, events, iic);
		}
	}

	public static void InstanceIdChangeTranslate(IContext context, AssetLookupSystem assetLookupSystem, GameManager gameManager, List<GameRulesEvent> changes, int index, MtgGameState oldState, MtgGameState newState, List<UXEvent> events, InstanceIdChange iic)
	{
		List<CardRevealedEvent> list = null;
		while (index + 1 < changes.Count && changes[index + 1] is CardRevealedEvent item)
		{
			if (list == null)
			{
				list = new List<CardRevealedEvent>();
			}
			changes.RemoveAt(index + 1);
			list.Add(item);
		}
		if (!(changes[index + 1] is ZoneChangeEvent zoneChangeEvent) || zoneChangeEvent.Id != iic.NewId)
		{
			return;
		}
		changes.Remove(zoneChangeEvent);
		MtgCardInstance mtgCardInstance = oldState.GetCardById(iic.OldId);
		if (mtgCardInstance == null && newState.TryGetCard(iic.OldId, out var card))
		{
			mtgCardInstance = card;
		}
		MtgCardInstance mtgCardInstance2 = newState.GetCardById(iic.NewId);
		bool flag = oldState.ObjectIds.Contains(iic.OldId);
		bool flag2 = newState.ObjectIds.Contains(iic.NewId);
		bool flag3 = mtgCardInstance2 == null && flag && !flag2;
		if (mtgCardInstance2 == null && !flag3 && flag2 && (!newState.Limbo.CardIds.Contains(iic.NewId) || IsOldIdForFutureZoneTransfer(iic.NewId, index + 1, changes)))
		{
			MtgZone newZone = zoneChangeEvent.NewZone;
			if (newZone.Owner == null)
			{
				MtgPlayer mtgPlayer = mtgCardInstance?.Owner ?? zoneChangeEvent.OldZone.Owner;
				mtgCardInstance2 = new MtgCardInstance
				{
					InstanceId = zoneChangeEvent.Id,
					Zone = newZone,
					Owner = mtgPlayer,
					Controller = mtgPlayer,
					CatalogId = WellKnownCatalogId.StandardCardBack
				};
			}
			else
			{
				mtgCardInstance2 = MtgCardInstance.UnknownCardData(zoneChangeEvent.Id, zoneChangeEvent.NewZone);
			}
		}
		if (mtgCardInstance2 != null || zoneChangeEvent.OldZoneType != ZoneType.Suppressed)
		{
			MtgCardInstance instigator = ZoneChangeEventTranslator.FindInstigator(changes, index, iic.OldId, newState, oldState, zoneChangeEvent);
			events.Add(new ZoneTransferUXEvent(context, assetLookupSystem, gameManager, iic.OldId, mtgCardInstance, iic.NewId, mtgCardInstance2, instigator, zoneChangeEvent.OldZone, zoneChangeEvent.NewZone, zoneChangeEvent.Reason, list));
		}
	}

	private static bool IsOldIdForFutureZoneTransfer(uint cardId, int startIdx, IReadOnlyList<GameRulesEvent> events)
	{
		foreach (InstanceIdChange item in IdChanges(startIdx, events ?? Array.Empty<GameRulesEvent>()))
		{
			if (item.OldId == cardId)
			{
				return true;
			}
		}
		return false;
	}

	private static IEnumerable<InstanceIdChange> IdChanges(int startIdx, IReadOnlyList<GameRulesEvent> events)
	{
		for (int i = startIdx; i < events.Count; i++)
		{
			if (events[i] is InstanceIdChange instanceIdChange)
			{
				yield return instanceIdChange;
			}
		}
	}

	private void CardDeletedEvent_EvtTranslate(MtgGameState oldState, List<UXEvent> events, CardDeletedEvent cde)
	{
		if (!oldState.VisibleCards.TryGetValue(cde.CardId, out var value) && !oldState.ObjectIds.Contains(cde.CardId))
		{
			return;
		}
		MtgZone mtgZone = null;
		if (value == null)
		{
			foreach (MtgZone value2 in oldState.Zones.Values)
			{
				if (value2.CardIds.Contains(cde.CardId))
				{
					mtgZone = value2;
					break;
				}
			}
			value = MtgCardInstance.UnknownCardData(cde.CardId, mtgZone);
		}
		MtgCardInstance instigator = null;
		ZoneTransferReason reason = ZoneTransferReason.Delete;
		if (value.ObjectType == GameObjectType.Ability)
		{
			for (int num = events.Count - 1; num >= 0; num--)
			{
				if (events[num] is ResolutionEventStartedUXEvent resolutionEventStartedUXEvent)
				{
					instigator = resolutionEventStartedUXEvent.Instigator;
					reason = ZoneTransferReason.Countered;
					break;
				}
				if (events[num] is ResolutionEventEndedUXEvent)
				{
					break;
				}
			}
		}
		ZoneTransferUXEvent zoneTransferUXEvent = new ZoneTransferUXEvent(_context, _assetLookupSystem, _gameManager, cde.CardId, value, cde.CardId, null, instigator, mtgZone, null, reason);
		if (MutationMergeUXEvent.IsZoneTransferDueToMutate(zoneTransferUXEvent, _gameManager))
		{
			events.Add(new MutationMergeUXEvent(zoneTransferUXEvent.OldId, zoneTransferUXEvent.OldInstance, _gameManager, _gameManager.CardDatabase, _gameManager.SplineMovementSystem, _gameManager.AssetLookupSystem));
		}
		events.Add(zoneTransferUXEvent);
	}

	private void CardRevealedEvent_EvtTranslate(List<GameRulesEvent> changes, int index, List<UXEvent> events, CardRevealedEvent cardRevealedEvent)
	{
		List<CardRevealedEvent> list = new List<CardRevealedEvent> { cardRevealedEvent };
		RevealEventType eventType = cardRevealedEvent.EventType;
		while (index + 1 < changes.Count && changes[index + 1] is CardRevealedEvent cardRevealedEvent2 && eventType == cardRevealedEvent2.EventType)
		{
			bool flag = false;
			if (eventType == RevealEventType.Reveal)
			{
				if (cardRevealedEvent.AffectorId == cardRevealedEvent2.AffectorId)
				{
					flag = true;
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				break;
			}
			changes.RemoveAt(index + 1);
			list.Add(cardRevealedEvent2);
		}
		events.Add(new RevealCardsUXEvent(list, _gameManager.Context));
	}

	private void ToxicOrInfectCounterProducedUXEvent(List<UXEvent> events)
	{
		IEnumerable<(uint, uint)> enumerable = getToxicAndInfectSourceAndTarget(events, _gameManager);
		if (!enumerable.Any())
		{
			return;
		}
		List<UXEvent> list = new List<UXEvent>();
		foreach (var (sourceId, sinkId) in enumerable)
		{
			list.Add(new CounterProducedUXEvent(sourceId, sinkId, _gameManager, CounterType.Poison));
		}
		events.Insert(findFirstCountersChangedEvent(events), new ParallelPlaybackUXEvent(list));
		static int findFirstCountersChangedEvent(List<UXEvent> list2)
		{
			for (int i = 0; i < list2.Count; i++)
			{
				if (list2[i] is CountersChangedUXEvent)
				{
					CountersChangedUXEvent countersChangedUXEvent = list2[i] as CountersChangedUXEvent;
					if (countersChangedUXEvent.CounterType == CounterType.Poison && countersChangedUXEvent.AffectorId == 0)
					{
						return i;
					}
				}
			}
			return list2.Count - 1;
		}
		static IEnumerable<(uint, uint)> getToxicAndInfectSourceAndTarget(IEnumerable<UXEvent> dmgEvts, GameManager gameManager)
		{
			foreach (UXEvent dmgEvt in dmgEvts)
			{
				if (dmgEvt is UXEventDamageDealt { DamageType: DamageType.Combat, Amount: >0, Source: MtgCardInstance source, Target: MtgPlayer target } && source.Abilities.Exists((AbilityPrintingData x) => x.BaseId == 264 || x.Id == 91) && gameManager.ViewManager.TryGetCardView(source.InstanceId, out var cardView) && gameManager.CardHolderManager.TryGetCardHolder(GREPlayerNum.Invalid, CardHolderType.Battlefield, out IBattlefieldCardHolder result) && result.CardIsStackParent(cardView))
				{
					yield return (source.InstanceId, target.InstanceId);
				}
			}
		}
	}

	public void Dispose()
	{
		foreach (KeyValuePair<Type, IEventTranslator> eventTranslator in _eventTranslators)
		{
			if (eventTranslator.Value is IDisposable disposable)
			{
				disposable.Dispose();
			}
		}
	}
}
