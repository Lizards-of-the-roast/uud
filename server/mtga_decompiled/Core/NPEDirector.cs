using System;
using System.Collections.Generic;
using System.Linq;
using GreClient.Rules;
using Wizards.Mtga;
using Wotc;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtgo.Gre.External.Messaging;

public class NPEDirector : IDisposable
{
	protected SortedDictionary<uint, SortedDictionary<Phase, HashSet<Step>>> BI_Checkmarks;

	public static Random RANDOM = new Random();

	public NPE_Game GameSpecificConfiguration;

	public NPEController NPEController;

	private UXEventQueue _uxEventQueue;

	private TutorialSkippedFromInGameSignal _tutorialSkippedFromInGameSignal;

	protected SortedDictionary<uint, SortedDictionary<Phase, SortedDictionary<Step, Queue<UXEvent>>>> _timeperiodScriptedUX;

	protected Dictionary<uint, Dictionary<TriggerCondition, Dictionary<uint, Queue<UXEvent>>>> _triggerScriptedUX;

	protected SortedDictionary<uint, SortedDictionary<Phase, SortedDictionary<Step, Dictionary<InterceptionType, Interception>>>> _interceptionsByTimePeriod;

	protected Dictionary<InterceptionType, Interception> _everPresentInterceptions;

	private DeclareAttackerRequest _currentAttackReq;

	private DeclareBlockersRequest _currentBlockerDecision;

	private Queue<UXEvent> _delayedReleaseUX = new Queue<UXEvent>();

	private Dictionary<InterceptionType, Interception> _currentTimePeriodInterceptions;

	private MtgGameState _stateForReference;

	private uint _currentTurn;

	private bool _canPlayLand;

	private bool _canPlayCreature;

	private bool _canPlaySpell;

	private Dictionary<uint, int> QuarryAttackCounts = new Dictionary<uint, int>();

	public bool Disposed { get; private set; }

	public bool CastForFree { get; private set; }

	public bool AllowsEndTurnButton { get; private set; }

	public bool AllowsStackingOnBattlefield { get; private set; }

	public bool ShowPhaseLadder { get; private set; }

	public bool ShouldBePaused { get; internal set; }

	private NPEState _npeState { get; }

	public NPEDirector(NPEController controller, NPEState npeState, UXEventQueue uxEventQueue, NPE_Game script)
	{
		script.InjectedNpeDirector = this;
		GameSpecificConfiguration = script;
		_timeperiodScriptedUX = script.ScriptedUX;
		_triggerScriptedUX = script.TriggerableScriptedUX;
		_interceptionsByTimePeriod = script.InterceptionsByTimePeriod;
		_everPresentInterceptions = script.EverPresentInterceptions;
		NPEController = controller;
		_npeState = npeState;
		_uxEventQueue = uxEventQueue;
		ShouldBePaused = true;
		CastForFree = script.CastForFree;
		AllowsEndTurnButton = script.ShowEndTurnButtonToPlayer;
		ShowPhaseLadder = script.ShowPhaseLadder;
		BI_Checkmarks = new SortedDictionary<uint, SortedDictionary<Phase, HashSet<Step>>>();
		_tutorialSkippedFromInGameSignal = Pantry.Get<TutorialSkippedFromInGameSignal>();
		_tutorialSkippedFromInGameSignal.Listeners += OnTutorialSkippedFromInGame;
	}

	public void Dispose()
	{
		OnDisposed(manuallyDisposing: true);
		Disposed = true;
		GC.SuppressFinalize(this);
	}

	protected void OnDisposed(bool manuallyDisposing)
	{
		if (!Disposed && manuallyDisposing)
		{
			if (GameSpecificConfiguration != null)
			{
				GameSpecificConfiguration.InjectedNpeDirector = null;
				GameSpecificConfiguration = null;
			}
			NPEController = null;
			_uxEventQueue = null;
			if (_tutorialSkippedFromInGameSignal != null)
			{
				_tutorialSkippedFromInGameSignal.Listeners -= OnTutorialSkippedFromInGame;
				_tutorialSkippedFromInGameSignal = null;
			}
		}
	}

	~NPEDirector()
	{
		OnDisposed(manuallyDisposing: false);
	}

	private NPEDirector GetNpeDirector()
	{
		return this;
	}

	public void React_IncomingRequest(BaseUserRequest req)
	{
		PlayPreviouslyTriggeredUX();
		if (!(req is DeclareAttackerRequest currentAttackReq))
		{
			if (!(req is DeclareBlockersRequest currentBlockerDecision))
			{
				if (!(req is ActionsAvailableRequest actionsAvailableRequest))
				{
					if (!(req is SelectTargetsRequest selectTargetsRequest))
					{
						if (!(req is SelectNRequest selectNRequest) || !GameSpecificConfiguration.ChooseReminders.TryGetValue(_stateForReference.GameWideTurn, out var value))
						{
							return;
						}
						SelectNRequest selectNRequest2 = selectNRequest;
						value.SparkySuggestedInstances.Clear();
						if (value.KeyCreatures.Count != 0)
						{
							foreach (uint id in selectNRequest2.Ids)
							{
								MtgCardInstance cardById = _stateForReference.GetCardById(id);
								if (cardById != null && value.KeyCreatures.Contains(cardById.GrpId))
								{
									value.SparkySuggestedInstances.Add(id);
								}
							}
						}
						else
						{
							value.SparkySuggestedInstances.Add(selectNRequest2.Ids.First());
						}
						_uxEventQueue.EnqueuePending(new NPEReminderUXEvent(GetNpeDirector, value));
					}
					else
					{
						if (!GameSpecificConfiguration.TargetReminders.TryGetValue(_stateForReference.GameWideTurn, out var value2))
						{
							return;
						}
						SelectTargetsRequest selectTargetsRequest2 = selectTargetsRequest;
						value2.SparkySuggestedInstances.Clear();
						if (value2.KeyCreatures.Count != 0)
						{
							foreach (TargetSelection targetSelection in selectTargetsRequest2.TargetSelections)
							{
								foreach (Target target in targetSelection.Targets)
								{
									MtgCardInstance cardById2 = _stateForReference.GetCardById(target.TargetInstanceId);
									if (cardById2 != null && value2.KeyCreatures.Contains(cardById2.GrpId))
									{
										value2.SparkySuggestedInstances.Add(target.TargetInstanceId);
									}
								}
							}
						}
						else
						{
							value2.SparkySuggestedInstances.Add(selectTargetsRequest2.TargetSelections.First().TargetIdx);
						}
						_uxEventQueue.EnqueuePending(new NPEReminderUXEvent(GetNpeDirector, value2));
					}
					return;
				}
				actionsAvailableRequest.Actions.RemoveAll((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.ActionType == ActionType.ActivateMana);
				List<Wotc.Mtgo.Gre.External.Messaging.Action> list = actionsAvailableRequest.Actions.FindAll((Wotc.Mtgo.Gre.External.Messaging.Action x) => ActionsAvailableRequest.IsCastAction(x));
				List<Wotc.Mtgo.Gre.External.Messaging.Action> list2 = list.FindAll((Wotc.Mtgo.Gre.External.Messaging.Action c) => c.ActionType == ActionType.Play);
				List<Wotc.Mtgo.Gre.External.Messaging.Action> list3 = list.FindAll((Wotc.Mtgo.Gre.External.Messaging.Action c) => c.ActionType == ActionType.Cast && CanAffordAction(c));
				List<Wotc.Mtgo.Gre.External.Messaging.Action> castCreatureActions = list3.FindAll((Wotc.Mtgo.Gre.External.Messaging.Action c) => _stateForReference.GetCardById(c.InstanceId).CardTypes.Contains(CardType.Creature));
				List<Wotc.Mtgo.Gre.External.Messaging.Action> list4 = list3.FindAll((Wotc.Mtgo.Gre.External.Messaging.Action x) => !castCreatureActions.Contains(x));
				_canPlayCreature = castCreatureActions.Count > 0;
				_canPlayLand = list2.Count > 0;
				_canPlaySpell = list4.Count > 0;
				if (!GameSpecificConfiguration.AvailableActionReminders.TryGetValue(_stateForReference.GameWideTurn, out var value3) || !value3.TryGetValue(_stateForReference.CurrentPhase, out var value4) || !value4.TryGetValue(_stateForReference.CurrentStep, out var value5))
				{
					return;
				}
				if (_canPlayLand && value5.TryGetValue(ActionReminderType.PlayALand, out var value6))
				{
					value6.SparkySuggestedInstances.Clear();
					value6.SparkySuggestedInstances.Add(list2.Select((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.InstanceId).First());
					_uxEventQueue.EnqueuePending(new NPEReminderUXEvent(GetNpeDirector, value6));
				}
				else if (_canPlaySpell && value5.TryGetValue(ActionReminderType.CastASpell, out value6))
				{
					value6.SparkySuggestedInstances.Clear();
					value6.SparkySuggestedInstances.Add(list4.Select((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.InstanceId).First());
					_uxEventQueue.EnqueuePending(new NPEReminderUXEvent(GetNpeDirector, value6));
				}
				else if (_canPlayCreature && value5.TryGetValue(ActionReminderType.CastACreature, out value6))
				{
					value6.SparkySuggestedInstances.Clear();
					value6.SparkySuggestedInstances.Add(castCreatureActions.Select((Wotc.Mtgo.Gre.External.Messaging.Action x) => x.InstanceId).First());
					_uxEventQueue.EnqueuePending(new NPEReminderUXEvent(GetNpeDirector, value6));
				}
				return;
			}
			if (NPEController._SparkyCombatState == CombatState.None)
			{
				_uxEventQueue.EnqueuePending(new ToggleCombatUXEvent(GetNpeDirector, CombatState.CombatBegun));
			}
			_currentBlockerDecision = currentBlockerDecision;
			IEnumerable<Blocker> enumerable = _currentBlockerDecision.AllBlockers.Where((Blocker x) => x.SelectedAttackerInstanceIds.Count == 0);
			IEnumerable<Blocker> enumerable2 = _currentBlockerDecision.AllBlockers.Where((Blocker x) => x.SelectedAttackerInstanceIds.Count != 0);
			if (!GameSpecificConfiguration.CombatReminders.TryGetValue(_stateForReference.GameWideTurn, out var value7) || !(value7 is DeclareReminder))
			{
				return;
			}
			DeclareReminder declareReminder = (DeclareReminder)value7;
			declareReminder.SparkySuggestedInstances.Clear();
			List<uint> list5 = new List<uint>();
			if (declareReminder.KeyCreatures.Count != 0)
			{
				List<uint> list6 = new List<uint>();
				foreach (uint keyCreature in declareReminder.KeyCreatures)
				{
					list6.Add(keyCreature);
				}
				foreach (Blocker item in enumerable2)
				{
					MtgCardInstance cardById3 = _stateForReference.GetCardById(item.BlockerInstanceId);
					if (list6.Contains(cardById3.GrpId))
					{
						list6.Remove(cardById3.GrpId);
					}
				}
				foreach (Blocker item2 in enumerable)
				{
					MtgCardInstance cardById4 = _stateForReference.GetCardById(item2.BlockerInstanceId);
					if (cardById4 != null && list6.Contains(cardById4.GrpId))
					{
						list5.Add(item2.BlockerInstanceId);
						list6.Remove(cardById4.GrpId);
					}
				}
				declareReminder.SparkySuggestedInstances.AddRange(list5);
			}
			else
			{
				IEnumerable<uint> collection = enumerable.Select((Blocker x) => x.BlockerInstanceId);
				declareReminder.SparkySuggestedInstances.AddRange(collection);
			}
			_uxEventQueue.EnqueuePending(new NPEReminderUXEvent(GetNpeDirector, declareReminder));
			return;
		}
		if (NPEController._SparkyCombatState == CombatState.None)
		{
			_uxEventQueue.EnqueuePending(new ToggleCombatUXEvent(GetNpeDirector, CombatState.CombatBegun));
		}
		_currentAttackReq = currentAttackReq;
		IEnumerable<uint> enumerable3 = _currentAttackReq.Attackers.Select((Attacker x) => x.AttackerInstanceId).Except(_currentAttackReq.DeclaredAttackers.Select((Attacker x) => x.AttackerInstanceId)).ToList();
		if (!GameSpecificConfiguration.CombatReminders.TryGetValue(_stateForReference.GameWideTurn, out var value8))
		{
			return;
		}
		if (value8 is DeclareReminder)
		{
			DeclareReminder declareReminder2 = (DeclareReminder)value8;
			declareReminder2.SparkySuggestedInstances.Clear();
			List<uint> list7 = new List<uint>();
			if (declareReminder2.KeyCreatures.Count != 0)
			{
				foreach (uint item3 in enumerable3)
				{
					MtgCardInstance cardById5 = _stateForReference.GetCardById(item3);
					if (cardById5 != null && declareReminder2.KeyCreatures.Contains(cardById5.GrpId))
					{
						list7.Add(item3);
					}
					if (cardById5 != null && AIUtilities.HasFlying(cardById5) && declareReminder2.KeyCreatures.Contains(1u))
					{
						list7.Add(item3);
					}
				}
				declareReminder2.SparkySuggestedInstances.AddRange(list7);
			}
			else
			{
				declareReminder2.SparkySuggestedInstances.AddRange(enumerable3);
			}
		}
		_uxEventQueue.EnqueuePending(new NPEReminderUXEvent(GetNpeDirector, value8));
	}

	public void React_OutgoingMessage(ClientToGREMessage outgoingMessage)
	{
		if (PlayerTookARealAction(outgoingMessage))
		{
			NPEController.ClearReminder();
			SkipUXEventsUntilNewGamestate();
			if (outgoingMessage.Type == ClientMessageType.SubmitAttackersReq && _triggerScriptedUX.TryGetValue(_stateForReference.GameWideTurn, out var value) && value.TryGetValue(TriggerCondition.SubmittedForAttack, out var value2))
			{
				foreach (KeyValuePair<uint, Queue<UXEvent>> item in value2)
				{
					QueueUpNPEUXEvents(item.Value);
				}
			}
		}
		if (outgoingMessage.Type == ClientMessageType.DeclareAttackersResp || outgoingMessage.Type == ClientMessageType.DeclareBlockersResp)
		{
			_uxEventQueue.EnqueuePending(new ToggleCombatUXEvent(GetNpeDirector, CombatState.CreaturesActive));
		}
	}

	public void AttackHaloTabulation(uint quarryId, int valuechange)
	{
		if (QuarryAttackCounts.ContainsKey(quarryId))
		{
			QuarryAttackCounts[quarryId] += valuechange;
		}
		else
		{
			QuarryAttackCounts[quarryId] = valuechange;
		}
		UpdateHalosInDeclareAttackers();
	}

	private void UpdateHalosInDeclareAttackers()
	{
		if (QuarryAttackCounts.TryGetValue(_stateForReference.Opponent.InstanceId, out var value))
		{
			NPEController.TopQuarryHalo.SetActive(value > 0);
		}
		if (QuarryAttackCounts.TryGetValue(_stateForReference.LocalPlayer.InstanceId, out var value2))
		{
			NPEController.BottomQuarryHalo.SetActive(value2 > 0);
		}
	}

	private void UpdateHalosInDeclareBlockers()
	{
		bool topActive = HasUnblockedAttacker(_stateForReference.LocalPlayerBattlefieldCards);
		bool bottomActive = HasUnblockedAttacker(_stateForReference.OpponentBattlefieldCards);
		_uxEventQueue.EnqueuePending(new QuarryHaloUXEvent(GetNpeDirector, topActive, bottomActive));
	}

	private bool HasUnblockedAttacker(IReadOnlyList<MtgCardInstance> battlefieldCards)
	{
		foreach (MtgCardInstance battlefieldCard in battlefieldCards)
		{
			if (battlefieldCard.AttackState == AttackState.Attacking && battlefieldCard.BlockedByIds.Count == 0)
			{
				return true;
			}
		}
		return false;
	}

	public void React_GameStateUpdate(GreClient.Rules.GameStateUpdate update, EntityViewManager viewManager)
	{
		if (_npeState.SkipTutorialButtonLocked)
		{
			_uxEventQueue.EnqueuePending(new UnlockTutorialButtonUXEvent(_npeState));
		}
		_stateForReference = update.NewState;
		if (_currentTurn != update.NewState.GameWideTurn)
		{
			ResetPerTurnConsiderations();
			_currentTurn = update.NewState.GameWideTurn;
		}
		if (!BI_Checkmarks.TryGetValue(_currentTurn, out var value))
		{
			value = new SortedDictionary<Phase, HashSet<Step>>();
			BI_Checkmarks.Add(_currentTurn, value);
		}
		Phase currentPhase = _stateForReference.CurrentPhase;
		if (currentPhase == Phase.Main1 || currentPhase == Phase.Combat || currentPhase == Phase.Main2)
		{
			if (!value.TryGetValue(currentPhase, out var value2))
			{
				value2 = new HashSet<Step>();
				value.Add(currentPhase, value2);
			}
			Step currentStep = _stateForReference.CurrentStep;
			if ((currentStep == Step.None || currentStep == Step.DeclareAttack || currentStep == Step.DeclareBlock || currentStep == Step.CombatDamage) && !value2.Contains(currentStep))
			{
				value2.Add(currentStep);
				_npeState.BI_NPEProgressUpdate(new NPEState.NPEProgressContext(NPEState.NPEProgressMarker.In_Game, _npeState.ActiveNPEGameNumber, _currentTurn, currentPhase, currentStep));
			}
		}
		PlayPreviouslyTriggeredUX();
		CheckForTriggeredUXEvents(update, viewManager);
		if (_stateForReference.CurrentPhase != Phase.Combat)
		{
			_uxEventQueue.EnqueuePending(new ToggleCombatUXEvent(GetNpeDirector, CombatState.None));
			_uxEventQueue.EnqueuePending(new QuarryHaloUXEvent(GetNpeDirector, topActive: false, bottomActive: false));
		}
		else
		{
			if (update.NewState.CurrentStep == Step.EndCombat)
			{
				QuarryAttackCounts = new Dictionary<uint, int>();
			}
			if (update.NewState.CurrentStep == Step.DeclareBlock)
			{
				UpdateHalosInDeclareBlockers();
			}
		}
		if (_timeperiodScriptedUX.TryGetValue(_stateForReference.GameWideTurn, out var value3) && value3.TryGetValue(_stateForReference.CurrentPhase, out var value4) && value4.TryGetValue(_stateForReference.CurrentStep, out var value5))
		{
			QueueUpNPEUXEvents(value5);
		}
		_currentTimePeriodInterceptions = null;
		if (_interceptionsByTimePeriod.TryGetValue(_stateForReference.GameWideTurn, out var value6) && value6.TryGetValue(_stateForReference.CurrentPhase, out var value7))
		{
			value7.TryGetValue(_stateForReference.CurrentStep, out _currentTimePeriodInterceptions);
		}
	}

	private void QueueUpNPEUXEvents(Queue<UXEvent> npeUXEvents)
	{
		foreach (UXEvent npeUXEvent in npeUXEvents)
		{
			_uxEventQueue.EnqueuePending(npeUXEvent);
		}
		npeUXEvents.Clear();
	}

	private void ResetPerTurnConsiderations()
	{
		_canPlayLand = false;
		_canPlayCreature = false;
		_canPlaySpell = false;
	}

	public bool CanAffordAction(Wotc.Mtgo.Gre.External.Messaging.Action action)
	{
		if (!CastForFree)
		{
			return action.AutoTapActions().Count > 0;
		}
		return true;
	}

	public bool WarnForBadChoice(uint id)
	{
		if (_everPresentInterceptions.TryGetValue(InterceptionType.ShouldChooseLand, out var value) && !_stateForReference.GetCardById(id).CardTypes.Contains(CardType.Land))
		{
			IssueWarning(value);
			return true;
		}
		return false;
	}

	public bool WarnForBadTarget(Target target)
	{
		if (_everPresentInterceptions.TryGetValue(InterceptionType.BadTargetting, out var value) && target.Highlight == Wotc.Mtgo.Gre.External.Messaging.HighlightType.Cold)
		{
			IssueWarning(value);
			return true;
		}
		if (_everPresentInterceptions.TryGetValue(InterceptionType.BadAuraTargetting, out value) && _stateForReference.GetCardById(_stateForReference.Stack.CardIds[0]).CardTypes.Contains(CardType.Enchantment))
		{
			MtgCardInstance cardById = _stateForReference.GetCardById(target.TargetInstanceId);
			if (cardById != null && (cardById.AffectedByQualifications.Exists((QualificationData x) => x.Type == QualificationType.CantUntap) || !cardById.Controller.IsLocalPlayer))
			{
				IssueWarning(value);
				return true;
			}
		}
		return false;
	}

	private System.Action BlockButtonFunctionality(Dictionary<InterceptionType, Interception> interceptionMap, IReadOnlyList<Blocker> blockers)
	{
		System.Action action3;
		if (interceptionMap.TryGetValue(InterceptionType.MissingKeyBlock, out var currentInterception))
		{
			HashSet<uint> hashSet = new HashSet<uint>();
			HashSet<uint> hashSet2 = new HashSet<uint>();
			foreach (Blocker blocker in blockers)
			{
				foreach (uint selectedAttackerInstanceId in blocker.SelectedAttackerInstanceIds)
				{
					hashSet2.Add(selectedAttackerInstanceId);
				}
			}
			if (_stateForReference.AttackInfo != null)
			{
				foreach (KeyValuePair<uint, AttackInfo> item in _stateForReference.AttackInfo)
				{
					hashSet.Add(item.Key);
				}
			}
			IEnumerable<uint> source = hashSet2.Select((uint x) => _stateForReference.GetCardById(x).GrpId);
			IEnumerable<uint> source2 = hashSet.Select((uint x) => _stateForReference.GetCardById(x).GrpId);
			System.Action action2 = default(System.Action);
			foreach (uint keyCard in currentInterception.KeyCards)
			{
				if (source.Contains(keyCard) || !source2.Contains(keyCard))
				{
					continue;
				}
				System.Action action = action2;
				if (action == null)
				{
					action3 = (action2 = delegate
					{
						IssueWarning(currentInterception);
					});
					action = action3;
				}
				action3 = action;
				goto IL_0296;
			}
		}
		if (interceptionMap.TryGetValue(InterceptionType.OverBlockingIsBad, out currentInterception))
		{
			Dictionary<uint, uint> dictionary = new Dictionary<uint, uint>();
			foreach (Blocker blocker2 in blockers)
			{
				foreach (uint selectedAttackerInstanceId2 in blocker2.SelectedAttackerInstanceIds)
				{
					if (dictionary.TryGetValue(selectedAttackerInstanceId2, out var value))
					{
						dictionary[selectedAttackerInstanceId2] = value + 1;
					}
					else
					{
						dictionary.Add(selectedAttackerInstanceId2, 1u);
					}
				}
			}
			foreach (KeyValuePair<uint, uint> item2 in dictionary)
			{
				MtgCardInstance cardById = _stateForReference.GetCardById(item2.Key);
				if (!currentInterception.KeyCards.Contains(cardById.GrpId) || item2.Value <= 1)
				{
					continue;
				}
				action3 = delegate
				{
					IssueWarning(currentInterception);
				};
				goto IL_0296;
			}
		}
		return null;
		IL_0296:
		return action3;
	}

	public System.Action BlockButtonFunctionality(IReadOnlyList<Blocker> blockers)
	{
		System.Action action = null;
		if (_currentTimePeriodInterceptions != null)
		{
			action = BlockButtonFunctionality(_currentTimePeriodInterceptions, blockers);
			if (action != null)
			{
				return action;
			}
		}
		action = BlockButtonFunctionality(_everPresentInterceptions, blockers);
		if (action != null)
		{
			return action;
		}
		return null;
	}

	public System.Action AttackWithAllButtonFunctionality()
	{
		System.Action action = null;
		if (_currentTimePeriodInterceptions != null)
		{
			action = AttackWithAllButtonFunctionality(_currentTimePeriodInterceptions);
			if (action != null)
			{
				return action;
			}
		}
		action = AttackWithAllButtonFunctionality(_everPresentInterceptions);
		if (action != null)
		{
			return action;
		}
		return null;
	}

	private System.Action AttackWithAllButtonFunctionality(Dictionary<InterceptionType, Interception> interceptionMap)
	{
		if (interceptionMap.TryGetValue(InterceptionType.BadAttack, out var currentInterception))
		{
			return delegate
			{
				IssueWarning(currentInterception);
			};
		}
		return null;
	}

	public System.Action SubmitAttackersButtonFunctionality(Dictionary<uint, Attacker> declaredAttackerMap)
	{
		System.Action action = null;
		if (_currentTimePeriodInterceptions != null)
		{
			action = SubmitAttackersButtonFunctionality(_currentTimePeriodInterceptions, declaredAttackerMap);
			if (action != null)
			{
				return action;
			}
		}
		action = SubmitAttackersButtonFunctionality(_everPresentInterceptions, declaredAttackerMap);
		if (action != null)
		{
			return action;
		}
		return null;
	}

	private System.Action SubmitAttackersButtonFunctionality(Dictionary<InterceptionType, Interception> interceptionMap, Dictionary<uint, Attacker> declaredAttackersMap)
	{
		if (interceptionMap.TryGetValue(InterceptionType.NotAttackingWithAll, out var currentInterception))
		{
			int num = 0;
			foreach (uint key in declaredAttackersMap.Keys)
			{
				if (declaredAttackersMap[key].SelectedDamageRecipient != null)
				{
					num++;
				}
			}
			if (_currentAttackReq.DeclaredAttackers.Count > num)
			{
				return delegate
				{
					IssueWarning(currentInterception);
				};
			}
		}
		System.Action action3;
		if (interceptionMap.TryGetValue(InterceptionType.MissingKeyAttacker, out currentInterception))
		{
			System.Action action2 = default(System.Action);
			foreach (uint key2 in declaredAttackersMap.Keys)
			{
				Attacker attacker = declaredAttackersMap[key2];
				MtgCardInstance cardById = _stateForReference.GetCardById(key2);
				if ((!currentInterception.KeyCards.Contains(cardById.GrpId) && (!currentInterception.KeyCards.Contains(1u) || !AIUtilities.HasFlying(cardById))) || attacker.SelectedDamageRecipient != null)
				{
					continue;
				}
				System.Action action = action2;
				if (action == null)
				{
					action3 = (action2 = delegate
					{
						IssueWarning(currentInterception);
					});
					action = action3;
				}
				action3 = action;
				goto IL_01bb;
			}
		}
		if (interceptionMap.TryGetValue(InterceptionType.BadAttack, out currentInterception))
		{
			foreach (uint key3 in declaredAttackersMap.Keys)
			{
				if (declaredAttackersMap[key3].SelectedDamageRecipient == null)
				{
					continue;
				}
				action3 = delegate
				{
					IssueWarning(currentInterception);
				};
				goto IL_01bb;
			}
		}
		return null;
		IL_01bb:
		return action3;
	}

	public System.Action NoAttackersButtonFunctionality(Dictionary<uint, List<Attacker>> attackerMap)
	{
		System.Action action = null;
		if (_currentTimePeriodInterceptions != null)
		{
			action = NoAttackersButtonFunctionality(_currentTimePeriodInterceptions, attackerMap);
			if (action != null)
			{
				return action;
			}
		}
		action = NoAttackersButtonFunctionality(_everPresentInterceptions, attackerMap);
		if (action != null)
		{
			return action;
		}
		return null;
	}

	private System.Action NoAttackersButtonFunctionality(Dictionary<InterceptionType, Interception> interceptionMap, Dictionary<uint, List<Attacker>> potentialAttackersMap)
	{
		if (interceptionMap.TryGetValue(InterceptionType.NotAttackingWithAll, out var currentInterception))
		{
			return delegate
			{
				IssueWarning(currentInterception);
			};
		}
		if (interceptionMap.TryGetValue(InterceptionType.MissingKeyAttacker, out currentInterception))
		{
			foreach (uint key in potentialAttackersMap.Keys)
			{
				foreach (Attacker item in potentialAttackersMap[key])
				{
					MtgCardInstance cardById = _stateForReference.GetCardById(key);
					if ((currentInterception.KeyCards.Contains(cardById.GrpId) || (currentInterception.KeyCards.Contains(1u) && AIUtilities.HasFlying(cardById))) && item.SelectedDamageRecipient == null)
					{
						return delegate
						{
							IssueWarning(currentInterception);
						};
					}
				}
			}
		}
		return null;
	}

	internal void Pause()
	{
		ShouldBePaused = true;
	}

	internal void Play()
	{
		ShouldBePaused = false;
	}

	private System.Action PromptButtonFunctionality(Dictionary<InterceptionType, Interception> interceptionMap)
	{
		if (_stateForReference.Stack.VisibleCards.Count == 0)
		{
			if (interceptionMap.TryGetValue(InterceptionType.PassingWithLandNotPlayed, out var currentInterception) && _canPlayLand)
			{
				return delegate
				{
					IssueWarning(currentInterception);
				};
			}
			if (interceptionMap.TryGetValue(InterceptionType.PassingWithCreatureNotCast, out currentInterception) && _canPlayCreature)
			{
				return delegate
				{
					IssueWarning(currentInterception);
				};
			}
			if (interceptionMap.TryGetValue(InterceptionType.PassingWithSpellNotCast, out currentInterception) && _canPlaySpell)
			{
				return delegate
				{
					IssueWarning(currentInterception);
				};
			}
		}
		return null;
	}

	public System.Action PromptButtonFunctionality()
	{
		System.Action action = null;
		if (_currentTimePeriodInterceptions != null)
		{
			action = PromptButtonFunctionality(_currentTimePeriodInterceptions);
			if (action != null)
			{
				return action;
			}
		}
		action = PromptButtonFunctionality(_everPresentInterceptions);
		if (action != null)
		{
			return action;
		}
		return null;
	}

	public void IssueWarning(Interception interception)
	{
		if (interception.DialogStrings.Count > 0)
		{
			MTGALocalizedString displayText = interception.DialogStrings[RANDOM.Next(interception.DialogStrings.Count)];
			_uxEventQueue.EnqueuePending(new NPEWarningUXEvent(GetNpeDirector, displayText, 3f));
		}
	}

	public Interception GetInterception()
	{
		if (!_everPresentInterceptions.TryGetValue(InterceptionType.CantAffordSpell, out var value))
		{
			return null;
		}
		return value;
	}

	public void ClearNPEUXPrompts()
	{
		NPEController.CurrentDialog?.Complete();
		NPEController.CurrentPause?.Complete();
		NPEController.CurrentHangerEvent?.Complete();
		_uxEventQueue.RemoveNpePauseUxEventsInPending();
		NPEController.HideAllPrompts();
	}

	public void SkipUXEventsUntilNewGamestate()
	{
		ClearNPEUXPrompts();
		NPEController.DropCurtain();
		_uxEventQueue.RemovaNpeUxEventsInPending();
	}

	private void PlayPreviouslyTriggeredUX()
	{
		QueueUpNPEUXEvents(_delayedReleaseUX);
	}

	private void CheckForTriggeredUXEvents(GreClient.Rules.GameStateUpdate gamestate, EntityViewManager viewManager)
	{
		List<GameRulesEvent> changes = gamestate.Changes;
		Dictionary<ZoneType, HashSet<uint>> dictionary = new Dictionary<ZoneType, HashSet<uint>>();
		HashSet<uint> hashSet = new HashSet<uint>();
		HashSet<uint> hashSet2 = new HashSet<uint>();
		HashSet<uint> hashSet3 = new HashSet<uint>();
		HashSet<uint> hashSet4 = new HashSet<uint>();
		foreach (GameRulesEvent item in changes)
		{
			if (item is ZoneChangeEvent)
			{
				ZoneChangeEvent zoneChangeEvent = item as ZoneChangeEvent;
				MtgCardInstance cardById = _stateForReference.GetCardById(zoneChangeEvent.Id);
				ZoneType type = zoneChangeEvent.NewZone.Type;
				if (cardById != null)
				{
					if (!dictionary.TryGetValue(type, out var value))
					{
						value = (dictionary[type] = new HashSet<uint>());
					}
					value.Add(cardById.GrpId);
				}
			}
			else if (item is DamageDealtEvent)
			{
				DamageDealtEvent damageDealtEvent = item as DamageDealtEvent;
				hashSet.Add(damageDealtEvent.Damager.GrpId);
				MtgCardInstance cardById2 = _stateForReference.GetCardById(damageDealtEvent.Victim.InstanceId);
				if (cardById2 != null)
				{
					hashSet2.Add(cardById2.GrpId);
				}
			}
			else if (item is CardCreatedEvent)
			{
				CardCreatedEvent cardCreatedEvent = item as CardCreatedEvent;
				if (cardCreatedEvent.Card.ObjectType == GameObjectType.Ability)
				{
					hashSet3.Add(cardCreatedEvent.Card.GrpId);
				}
			}
			else if (item is CardDeletedEvent)
			{
				CardDeletedEvent cardDeletedEvent = item as CardDeletedEvent;
				DuelScene_CDC cardView = viewManager.GetCardView(cardDeletedEvent.CardId);
				if (cardView != null && cardView.Model != null && cardView.Model.ObjectType == GameObjectType.Ability)
				{
					hashSet4.Add(cardView.Model.GrpId);
				}
			}
			else
			{
				if (!(item is GameEndEvent))
				{
					continue;
				}
				Queue<NPEDialogUXEvent> queue = null;
				queue = ((((GameEndEvent)item).Loser != GREPlayerNum.LocalPlayer) ? GameSpecificConfiguration.OnAIDefeatDialog : GameSpecificConfiguration.OnAIVictoryDialog);
				if (queue == null || _npeState.SkipTutorialWasQueuedUpFromInGame)
				{
					continue;
				}
				foreach (NPEDialogUXEvent item2 in queue)
				{
					_uxEventQueue.EnqueuePending(item2);
				}
			}
		}
		if (!_triggerScriptedUX.TryGetValue(_stateForReference.GameWideTurn, out var value2))
		{
			return;
		}
		foreach (KeyValuePair<TriggerCondition, Dictionary<uint, Queue<UXEvent>>> item3 in value2)
		{
			Dictionary<uint, Queue<UXEvent>> value3 = item3.Value;
			switch (item3.Key)
			{
			case TriggerCondition.EntersBattlefield:
			{
				if (dictionary.TryGetValue(ZoneType.Battlefield, out var value9))
				{
					MapToDelayedReleaseUXEvent(value9, value3);
				}
				break;
			}
			case TriggerCondition.SpellFinishesResolving:
			case TriggerCondition.Dies:
			{
				if (dictionary.TryGetValue(ZoneType.Graveyard, out var value7))
				{
					MapToDelayedReleaseUXEvent(value7, value3);
				}
				break;
			}
			case TriggerCondition.IsCast:
			{
				if (dictionary.TryGetValue(ZoneType.Stack, out var value8))
				{
					MapToDelayedReleaseUXEvent(value8, value3);
				}
				break;
			}
			case TriggerCondition.AbilityTriggersOrActivates:
				MapToDelayedReleaseUXEvent(hashSet3, value3);
				break;
			case TriggerCondition.BUGGEDAbilityFinishesAndLeavesTheStack:
				MapToDelayedReleaseUXEvent(hashSet4, value3);
				break;
			case TriggerCondition.IsDrawn:
			{
				if (dictionary.TryGetValue(ZoneType.Hand, out var value6))
				{
					MapToDelayedReleaseUXEvent(value6, value3);
				}
				break;
			}
			case TriggerCondition.DealsDamage:
				MapToDelayedReleaseUXEvent(hashSet, value3);
				break;
			case TriggerCondition.IsDealtDamage:
				MapToDelayedReleaseUXEvent(hashSet2, value3);
				break;
			case TriggerCondition.IsBlocking:
				foreach (uint objectId in gamestate.NewState.ObjectIds)
				{
					MtgCardInstance cardById4 = gamestate.NewState.GetCardById(objectId);
					if (cardById4 != null && cardById4.CardTypes.Contains(CardType.Creature) && cardById4.BlockState == BlockState.Blocking && value3.TryGetValue(cardById4.GrpId, out var value5))
					{
						while (value5.Count > 0)
						{
							_delayedReleaseUX.Enqueue(value5.Dequeue());
						}
					}
				}
				break;
			case TriggerCondition.IsAttacking:
				foreach (uint objectId2 in gamestate.NewState.ObjectIds)
				{
					MtgCardInstance cardById3 = gamestate.NewState.GetCardById(objectId2);
					if (cardById3 != null && cardById3.CardTypes.Contains(CardType.Creature) && cardById3.AttackState == AttackState.Attacking && value3.TryGetValue(cardById3.GrpId, out var value4))
					{
						while (value4.Count > 0)
						{
							_delayedReleaseUX.Enqueue(value4.Dequeue());
						}
					}
				}
				break;
			}
		}
	}

	private void MapToDelayedReleaseUXEvent(HashSet<uint> candidates, Dictionary<uint, Queue<UXEvent>> triggerableUXForCard)
	{
		foreach (uint candidate in candidates)
		{
			if (triggerableUXForCard.TryGetValue(candidate, out var value))
			{
				while (value.Count > 0)
				{
					_delayedReleaseUX.Enqueue(value.Dequeue());
				}
			}
		}
	}

	private bool PlayerTookARealAction(ClientToGREMessage msg)
	{
		if (msg != null)
		{
			if (msg.PerformActionResp != null)
			{
				foreach (Wotc.Mtgo.Gre.External.Messaging.Action action in msg.PerformActionResp.Actions)
				{
					if (action.ActionType == ActionType.Cast || action.ActionType == ActionType.Pass || action.ActionType == ActionType.Play)
					{
						return true;
					}
				}
			}
			if (msg.Type == ClientMessageType.SubmitAttackersReq || msg.Type == ClientMessageType.DeclareAttackersResp || msg.Type == ClientMessageType.DeclareBlockersResp || msg.Type == ClientMessageType.SubmitBlockersReq || msg.Type == ClientMessageType.SelectNresp || msg.Type == ClientMessageType.SubmitTargetsReq)
			{
				return true;
			}
		}
		return false;
	}

	private void OnTutorialSkippedFromInGame(SignalArgs args)
	{
		SkipUXEventsUntilNewGamestate();
	}
}
