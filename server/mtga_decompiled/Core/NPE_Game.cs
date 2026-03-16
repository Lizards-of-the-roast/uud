using System.Collections.Generic;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.DuelScene.UXEvents;
using Wotc.Mtgo.Gre.External.Messaging;

public abstract class NPE_Game
{
	public string Battlefield = "DAR";

	public bool ShowCinematicAtStart;

	protected const string HAS_FLYING = "HAS_FLYING";

	public const uint HAS_FLYING_FAUX_GRPID = 1u;

	private bool HumanGoesFirst = true;

	public bool CastForFree;

	public bool UseGame0Setup;

	public bool ShowPowerToughnessIcon = true;

	public Queue<NPEDialogUXEvent> OnAIDefeatDialog;

	public Queue<NPEDialogUXEvent> OnAIVictoryDialog;

	public bool ShowEndTurnButtonToPlayer;

	public bool ShowPhaseLadder = true;

	public NPEController.Actor SparkyPortrait = NPEController.Actor.Spark;

	public NPEController.Actor OpponentPortrait = NPEController.Actor.Elf;

	public SortedDictionary<uint, SortedDictionary<Phase, SortedDictionary<Step, Queue<UXEvent>>>> ScriptedUX;

	public Dictionary<uint, Dictionary<TriggerCondition, Dictionary<uint, Queue<UXEvent>>>> TriggerableScriptedUX;

	public SortedDictionary<uint, SortedDictionary<Phase, SortedDictionary<Step, Dictionary<InterceptionType, Interception>>>> InterceptionsByTimePeriod;

	public Dictionary<InterceptionType, Interception> EverPresentInterceptions;

	public SortedDictionary<uint, SortedDictionary<Phase, SortedDictionary<Step, Dictionary<ActionReminderType, AvailableActionReminder>>>> AvailableActionReminders;

	public SortedDictionary<uint, NPEReminder> CombatReminders;

	public SortedDictionary<uint, PickReminder> TargetReminders;

	public SortedDictionary<uint, PickReminder> ChooseReminders;

	public SortedDictionary<uint, SortedDictionary<Phase, SortedDictionary<Step, Queue<ScriptedAction>>>> _requestedPlays;

	public Dictionary<uint, bool> _freestylingAllowances;

	public List<uint> _topPicksForPayingCosts;

	public List<uint> _topPicksChooseActions;

	public List<uint> _requestedAsapPlays;

	public List<uint> _turnsToNotAttack;

	public List<uint> _turnsToAttackAll;

	public Dictionary<uint, List<uint>> _turnsToAttackWithCreature;

	public Dictionary<uint, TargetingPreferences> _targetPreferences;

	public List<uint> _creaturesToChumpWith;

	private static Dictionary<string, uint> EnglishTitleToGRPIDLookup = new Dictionary<string, uint>
	{
		{ "HAS_FLYING", 1u },
		{ "Blinding Radiance", 68766u },
		{ "Spiritual Guardian", 68767u },
		{ "Tactical Advantage", 68769u },
		{ "Feral Roar", 68771u },
		{ "Shorecomber Crab", 68772u },
		{ "Goblin Bruiser", 68773u },
		{ "Warren Pitboss", 68774u },
		{ "Zephyr Gull", 68776u },
		{ "Arcane Currents", 68777u },
		{ "Camp Infiltrator", 68780u },
		{ "Cut Down", 68781u },
		{ "Soulhunter Rakshasa", 68782u },
		{ "Raging Goblin", 68784u },
		{ "Confront the Assault", 68786u },
		{ "Take Vengeance", 68787u },
		{ "Knight's Pledge", 68788u },
		{ "Loxodon Line Breaker", 68789u },
		{ "Volcanic Dragon", 68790u },
		{ "Rise from the Grave", 68791u },
		{ "Waterknot", 68792u },
		{ "Serra Angel", 68793u },
		{ "Chaos Maw", 68794u },
		{ "Miasmic Mummy", 68795u },
		{ "Overflowing Insight", 68796u },
		{ "Earthquake", 68797u },
		{ "Ambition's Cost", 68798u },
		{ "Reave Soul", 68799u },
		{ "Divination", 68800u },
		{ "Rumbling Baloth", 68801u },
		{ "Goblin Grenade", 68802u },
		{ "Renegade Demon", 68803u },
		{ "Sanctuary Cat", 68804u },
		{ "Fortress Crab", 68805u },
		{ "Altar's Reap", 68806u },
		{ "Fleshbag Marauder", 68807u },
		{ "Goblin", 68808u },
		{ "Spirit", 68809u },
		{ "Crux of Fate", 68810u },
		{ "Seismic Rupture", 68811u },
		{ "Twincast", 68812u },
		{ "Murder", 68813u },
		{ "Doublecast", 68814u },
		{ "Angelic Reward", 69108u },
		{ "Hallowed Priest", 69109u },
		{ "Inspiring Commander", 69110u },
		{ "Shrine Keeper", 69111u },
		{ "River's Favor", 69112u },
		{ "Titanic Pelagosaur", 69113u },
		{ "Trapped in a Whirlpool", 69114u },
		{ "Cruel Cut", 69115u },
		{ "Nimble Pilferer", 69116u },
		{ "Goblin Gang Leader", 69117u },
		{ "Ogre Painbringer", 69118u },
		{ "Treetop Warden", 69119u },
		{ "Ancient Crab", 69441u },
		{ "Hired Blade", 69442u },
		{ "Plains", 69443u },
		{ "Island", 69444u },
		{ "Swamp", 69445u },
		{ "Mountain", 69446u },
		{ "Forest", 69447u }
	};

	public NPEDirector InjectedNpeDirector { get; set; }

	protected void SetBattlefield(string battlefieldId)
	{
		Battlefield = battlefieldId;
	}

	protected void SetStartingPlayer(Turn turn)
	{
		HumanGoesFirst = turn == Turn.Human;
	}

	protected void SetOpponentPortrait(NPEController.Actor actor)
	{
		OpponentPortrait = actor;
	}

	public void Initialize()
	{
		ScriptedUX = new SortedDictionary<uint, SortedDictionary<Phase, SortedDictionary<Step, Queue<UXEvent>>>>();
		TriggerableScriptedUX = new Dictionary<uint, Dictionary<TriggerCondition, Dictionary<uint, Queue<UXEvent>>>>();
		OnAIVictoryDialog = new Queue<NPEDialogUXEvent>();
		OnAIDefeatDialog = new Queue<NPEDialogUXEvent>();
		InterceptionsByTimePeriod = new SortedDictionary<uint, SortedDictionary<Phase, SortedDictionary<Step, Dictionary<InterceptionType, Interception>>>>();
		EverPresentInterceptions = new Dictionary<InterceptionType, Interception>();
		AvailableActionReminders = new SortedDictionary<uint, SortedDictionary<Phase, SortedDictionary<Step, Dictionary<ActionReminderType, AvailableActionReminder>>>>();
		CombatReminders = new SortedDictionary<uint, NPEReminder>();
		TargetReminders = new SortedDictionary<uint, PickReminder>();
		ChooseReminders = new SortedDictionary<uint, PickReminder>();
		_requestedPlays = new SortedDictionary<uint, SortedDictionary<Phase, SortedDictionary<Step, Queue<ScriptedAction>>>>();
		_freestylingAllowances = new Dictionary<uint, bool>();
		_requestedAsapPlays = new List<uint>();
		_topPicksForPayingCosts = new List<uint>();
		_topPicksChooseActions = new List<uint>();
		_turnsToNotAttack = new List<uint>();
		_turnsToAttackAll = new List<uint>();
		_turnsToAttackWithCreature = new Dictionary<uint, List<uint>>();
		_targetPreferences = new Dictionary<uint, TargetingPreferences>();
		_creaturesToChumpWith = new List<uint>();
		AddGameContent();
	}

	private NPEDirector GetNpeDirector()
	{
		return InjectedNpeDirector;
	}

	protected abstract void AddGameContent();

	protected uint AdjustTurnToGlobalTurn(Turn whoseTurn, uint turn)
	{
		uint num = turn * 2;
		if (whoseTurn == Turn.AI)
		{
			if (!HumanGoesFirst)
			{
				num--;
			}
		}
		else if (HumanGoesFirst)
		{
			num--;
		}
		return num;
	}

	protected void AddOnAIVictoryDialog(float duration, MTGALocalizedString text, NPEController.Actor character, string wwiseEvent)
	{
		OnAIVictoryDialog.Enqueue(new NPEDialogUXEvent(GetNpeDirector, character, text, wwiseEvent, duration));
	}

	protected void AddOnAIDefeatedDialog(float duration, MTGALocalizedString text, NPEController.Actor character, string wwiseEvent)
	{
		OnAIDefeatDialog.Enqueue(new NPEDialogUXEvent(GetNpeDirector, character, text, wwiseEvent, duration));
	}

	protected void PlayDialog(float duration, MTGALocalizedString text, NPEController.Actor character, string wwiseEvent, Turn whoseTurn, uint turn, Phase gamePhase = Phase.Main1, Step step = Step.None, bool followCard = false)
	{
		NPEDialogUXEvent scriptedUX = new NPEDialogUXEvent(GetNpeDirector, character, text, wwiseEvent, duration, followCard);
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddScriptedUX(scriptedUX, adjustedTurn, gamePhase, step);
	}

	protected void PlayFallTriggeringDialog(float duration, MTGALocalizedString text, NPEController.Actor character, string wwiseEvent, Turn whoseTurn, uint turn, Phase gamePhase = Phase.Main1, Step step = Step.None)
	{
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		NPEDialogUXEvent dialog = new NPEDialogUXEvent(GetNpeDirector, character, text, wwiseEvent, duration);
		AddScriptedUX(dialog, adjustedTurn, gamePhase, step);
		WaitUntilUXEvent scriptedUX = new WaitUntilUXEvent(dialogIsComplete);
		AddScriptedUX(scriptedUX, adjustedTurn, gamePhase, step);
		CallbackUXEvent scriptedUX2 = new CallbackUXEvent(eventCallback);
		AddScriptedUX(scriptedUX2, adjustedTurn, gamePhase, step);
		bool dialogIsComplete()
		{
			return dialog.IsComplete;
		}
		void eventCallback()
		{
			GetNpeDirector().NPEController.DropCurtain();
		}
	}

	protected void ShowHangerOnBattlefield(float duration, HangerSituation type, string targetCard, Turn whoseTurn, uint turn, Phase gamePhase = Phase.Main1, Step step = Step.None, bool showOnLeftSide = false)
	{
		NPEShowBattlefieldHangerUXEvent scriptedUX = new NPEShowBattlefieldHangerUXEvent(GetNpeDirector, duration, ConvertANATitleToGRPID(targetCard), type, showOnLeftSide);
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddScriptedUX(scriptedUX, adjustedTurn, gamePhase, step);
	}

	protected void DoPause(float duration, Turn whoseTurn, uint turn, Phase gamePhase = Phase.Main1, Step step = Step.None)
	{
		NPEPauseUXEvent scriptedUX = new NPEPauseUXEvent(GetNpeDirector, duration);
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddScriptedUX(scriptedUX, adjustedTurn, gamePhase, step);
	}

	protected void ShowTooltipBumper(float duration, MTGALocalizedString text, PromptType position, Turn whoseTurn, uint turn, Phase gamePhase = Phase.Main1, Step step = Step.None)
	{
		NPETooltipBumperUXEvent scriptedUX = new NPETooltipBumperUXEvent(GetNpeDirector, text, duration, position);
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddScriptedUX(scriptedUX, adjustedTurn, gamePhase, step);
	}

	protected void ShowDismissableDeluxeTooltip(DeluxeTooltipType type, Turn whoseTurn, uint turn, Phase gamePhase = Phase.Main1, Step step = Step.None)
	{
		NPEDismissableDeluxeTooltipUXEvent scriptedUX = new NPEDismissableDeluxeTooltipUXEvent(GetNpeDirector, type);
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddScriptedUX(scriptedUX, adjustedTurn, gamePhase, step);
	}

	private void AddScriptedUX(UXEvent scriptedUX, uint adjustedTurn, Phase gamePhase, Step step)
	{
		if (!ScriptedUX.TryGetValue(adjustedTurn, out var value))
		{
			value = new SortedDictionary<Phase, SortedDictionary<Step, Queue<UXEvent>>>();
			ScriptedUX.Add(adjustedTurn, value);
		}
		if (!value.TryGetValue(gamePhase, out var value2))
		{
			value2 = new SortedDictionary<Step, Queue<UXEvent>>();
			value.Add(gamePhase, value2);
		}
		if (!value2.TryGetValue(step, out var value3))
		{
			value3 = new Queue<UXEvent>();
			value2.Add(step, value3);
		}
		value3.Enqueue(scriptedUX);
	}

	protected void PlayTriggerableDialog(float duration, MTGALocalizedString text, NPEController.Actor character, string wwiseEvent, Turn whoseTurn, uint turn, string triggeringCard, TriggerCondition trigger)
	{
		NPEDialogUXEvent scriptedUX = new NPEDialogUXEvent(GetNpeDirector, character, text, wwiseEvent, duration);
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddTriggerableScriptedUX(scriptedUX, adjustedTurn, triggeringCard, trigger);
	}

	protected void ShowTriggerableTooltipBumper(float duration, MTGALocalizedString text, PromptType position, Turn whoseTurn, uint turn, string triggeringCard, TriggerCondition trigger, bool isBlocking = false)
	{
		NPETooltipBumperUXEvent scriptedUX = new NPETooltipBumperUXEvent(GetNpeDirector, text, duration, position, isBlocking);
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddTriggerableScriptedUX(scriptedUX, adjustedTurn, triggeringCard, trigger);
	}

	protected void TriggerDismissableDeluxeTooltip(DeluxeTooltipType type, Turn whoseTurn, uint turn, string triggeringCard, TriggerCondition trigger)
	{
		NPEDismissableDeluxeTooltipUXEvent scriptedUX = new NPEDismissableDeluxeTooltipUXEvent(GetNpeDirector, type);
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddTriggerableScriptedUX(scriptedUX, adjustedTurn, triggeringCard, trigger);
	}

	protected void TriggerableShowHangerOnBattlefield(float duration, HangerSituation type, Turn whoseTurn, uint turn, string triggeringCard, TriggerCondition trigger, bool showOnLeftSide = false, string cardToShowOn = null)
	{
		NPEShowBattlefieldHangerUXEvent scriptedUX = ((cardToShowOn == null) ? new NPEShowBattlefieldHangerUXEvent(GetNpeDirector, duration, ConvertANATitleToGRPID(triggeringCard), type, showOnLeftSide) : new NPEShowBattlefieldHangerUXEvent(GetNpeDirector, duration, ConvertANATitleToGRPID(cardToShowOn), type, showOnLeftSide));
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddTriggerableScriptedUX(scriptedUX, adjustedTurn, triggeringCard, trigger);
	}

	protected void TriggerablePTExplainer(float duration, bool showPower, string targetCard, Turn whoseTurn, uint turn)
	{
		NPEShowExplainerOnBattlefieldUXEvent scriptedUX = new NPEShowExplainerOnBattlefieldUXEvent(GetNpeDirector, duration, ConvertANATitleToGRPID(targetCard), showPower);
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddTriggerableScriptedUX(scriptedUX, adjustedTurn, targetCard, TriggerCondition.SubmittedForAttack);
	}

	protected void TriggerablePause(float duration, Turn whoseTurn, uint turn, string triggeringCard, TriggerCondition trigger)
	{
		NPEPauseUXEvent scriptedUX = new NPEPauseUXEvent(GetNpeDirector, duration);
		uint adjustedTurn = AdjustTurnToGlobalTurn(whoseTurn, turn);
		AddTriggerableScriptedUX(scriptedUX, adjustedTurn, triggeringCard, trigger);
	}

	private void AddTriggerableScriptedUX(UXEvent scriptedUX, uint adjustedTurn, string triggeringCard, TriggerCondition trigger)
	{
		if (!TriggerableScriptedUX.TryGetValue(adjustedTurn, out var value))
		{
			value = new Dictionary<TriggerCondition, Dictionary<uint, Queue<UXEvent>>>();
			TriggerableScriptedUX.Add(adjustedTurn, value);
		}
		if (!value.TryGetValue(trigger, out var value2))
		{
			value2 = new Dictionary<uint, Queue<UXEvent>>();
			value.Add(trigger, value2);
		}
		uint key = ConvertANATitleToGRPID(triggeringCard);
		if (!value2.TryGetValue(key, out var value3))
		{
			value3 = new Queue<UXEvent>();
			value2.Add(key, value3);
		}
		value3.Enqueue(scriptedUX);
	}

	protected void AddActionReminder(ActionReminderType type, MTGALocalizedString reminderText, float tooltipTime, float sparkyDispatchTime, Turn whoseTurn, uint turn, Phase gamePhase = Phase.Main1, Step step = Step.None)
	{
		AddActionReminder(type, reminderText, tooltipTime, sparkyDispatchTime, gamePhase, step, whoseTurn, turn);
	}

	protected void AddActionReminder(ActionReminderType type, MTGALocalizedString reminderText, float tooltipTime, float sparkyDispatchTime, params uint[] turns)
	{
		AddActionReminder(type, reminderText, tooltipTime, sparkyDispatchTime, Phase.Main1, Step.None, Turn.Human, turns);
		AddActionReminder(type, reminderText, tooltipTime, sparkyDispatchTime, Phase.Main2, Step.None, Turn.Human, turns);
	}

	private void AddActionReminder(ActionReminderType type, MTGALocalizedString reminderText, float tooltipTime, float sparkyDispatchTime, Phase gamePhase, Step step, Turn whoseTurn, params uint[] turns)
	{
		foreach (uint turn in turns)
		{
			uint key = AdjustTurnToGlobalTurn(whoseTurn, turn);
			if (!AvailableActionReminders.TryGetValue(key, out var value))
			{
				value = new SortedDictionary<Phase, SortedDictionary<Step, Dictionary<ActionReminderType, AvailableActionReminder>>>();
				AvailableActionReminders[key] = value;
			}
			if (!value.TryGetValue(gamePhase, out var value2))
			{
				value2 = (value[gamePhase] = new SortedDictionary<Step, Dictionary<ActionReminderType, AvailableActionReminder>>());
			}
			if (!value2.TryGetValue(step, out var value3))
			{
				value3 = (value2[step] = new Dictionary<ActionReminderType, AvailableActionReminder>());
			}
			if (!value3.ContainsKey(type))
			{
				value3.Add(type, new AvailableActionReminder(type, reminderText, tooltipTime, sparkyDispatchTime));
			}
		}
	}

	protected void AddBlockingDelayedReminder(MTGALocalizedString reminderText, MTGALocalizedString submitText, float tooltipTimeToWait, float sparkyTimeToWait, uint turn, params string[] keyBlockers)
	{
		uint key = AdjustTurnToGlobalTurn(Turn.AI, turn);
		CombatReminders[key] = new DeclareReminder(reminderText, submitText, tooltipTimeToWait, sparkyTimeToWait, ConvertANATitleToGRPID(keyBlockers));
	}

	protected void AddDontAttackReminder(MTGALocalizedString submitText, float tooltipTimeToWait, float sparkyTimeToWait, uint turn)
	{
		uint key = AdjustTurnToGlobalTurn(Turn.Human, turn);
		NoAttacksReminder value = new NoAttacksReminder(submitText, tooltipTimeToWait, sparkyTimeToWait);
		CombatReminders[key] = value;
	}

	protected void AddTargetReminder(MTGALocalizedString targetText, float tooltipTimeToWait, float sparkyTimeToWait, Turn whoseTurn, uint turn, params string[] keyTargs)
	{
		uint key = AdjustTurnToGlobalTurn(whoseTurn, turn);
		PickReminder value = new PickReminder(targetText, tooltipTimeToWait, sparkyTimeToWait, ConvertANATitleToGRPID(keyTargs));
		TargetReminders[key] = value;
	}

	protected void AddSelectReminder(MTGALocalizedString choiceText, float tooltipTimeToWait, float sparkyTimeToWait, Turn whoseTurn, uint turn, params string[] keyChoices)
	{
		uint key = AdjustTurnToGlobalTurn(whoseTurn, turn);
		PickReminder value = new PickReminder(choiceText, tooltipTimeToWait, sparkyTimeToWait, ConvertANATitleToGRPID(keyChoices));
		ChooseReminders[key] = value;
	}

	protected void AddAttackingReminder(MTGALocalizedString reminderText, MTGALocalizedString submitText, float tooltipTimeToWait, float sparkyTimeToWait, uint turn, params string[] keyAttackers)
	{
		uint key = AdjustTurnToGlobalTurn(Turn.Human, turn);
		CombatReminders[key] = new DeclareReminder(reminderText, submitText, tooltipTimeToWait, sparkyTimeToWait, ConvertANATitleToGRPID(keyAttackers));
	}

	protected void AddAlwaysOnInterceptorDialog(InterceptionType type, params MTGALocalizedString[] dialog)
	{
		AddInterceptorDialog(null, type, EverPresentInterceptions, dialog);
	}

	protected void AddInterceptorDialogForKeyCard(string KeyCard, InterceptionType type, MTGALocalizedString dialog, Phase gamePhase, Step step, Turn whoseTurn, params uint[] turns)
	{
		AddInterceptorDialog(KeyCard, type, dialog, gamePhase, step, whoseTurn, turns);
	}

	protected void AddInterceptorDialog(InterceptionType type, MTGALocalizedString dialog, Phase gamePhase, Step step, Turn whoseTurn, params uint[] turns)
	{
		AddInterceptorDialog(null, type, dialog, gamePhase, step, whoseTurn, turns);
	}

	private void AddInterceptorDialog(string KeyCard, InterceptionType type, MTGALocalizedString dialog, Phase gamePhase, Step step, Turn whoseTurn, params uint[] turns)
	{
		foreach (uint turn in turns)
		{
			uint key = AdjustTurnToGlobalTurn(whoseTurn, turn);
			if (!InterceptionsByTimePeriod.TryGetValue(key, out var value))
			{
				value = new SortedDictionary<Phase, SortedDictionary<Step, Dictionary<InterceptionType, Interception>>>();
				InterceptionsByTimePeriod.Add(key, value);
			}
			if (!value.TryGetValue(gamePhase, out var value2))
			{
				value2 = new SortedDictionary<Step, Dictionary<InterceptionType, Interception>>();
				value.Add(gamePhase, value2);
			}
			if (!value2.TryGetValue(step, out var value3))
			{
				value3 = new Dictionary<InterceptionType, Interception>();
				value2.Add(step, value3);
			}
			AddInterceptorDialog(KeyCard, type, value3, dialog);
		}
	}

	private void AddInterceptorDialog(string KeyCard, InterceptionType type, Dictionary<InterceptionType, Interception> interceptionMap, params MTGALocalizedString[] dialog)
	{
		if (!interceptionMap.TryGetValue(type, out var value))
		{
			value = new Interception();
			interceptionMap.Add(type, value);
		}
		foreach (MTGALocalizedString item in dialog)
		{
			value.DialogStrings.Add(item);
		}
		if (KeyCard != null)
		{
			value.KeyCards.Add(ConvertANATitleToGRPID(KeyCard));
		}
	}

	protected void AllowFreestyling(Turn whoseTurn, uint turn, bool direction)
	{
		_freestylingAllowances[AdjustTurnToGlobalTurn(whoseTurn, turn)] = direction;
	}

	protected void AI_Activates(Turn whoseTurn, uint turn, string cardname, Phase gamePhase = Phase.Main1, Step step = Step.None)
	{
		AddPlay(AdjustTurnToGlobalTurn(whoseTurn, turn), gamePhase, step, new TryActivateAction(ConvertANATitleToGRPID(cardname)));
	}

	protected void AI_Casts(Turn whoseTurn, uint turn, string cardname, Phase gamePhase = Phase.Main1, Step step = Step.None)
	{
		AddPlay(AdjustTurnToGlobalTurn(whoseTurn, turn), gamePhase, step, new TryCastAction(ConvertANATitleToGRPID(cardname)));
	}

	protected void AI_Use_TOC(Turn whoseTurn, uint turn, uint TOC_grpid, Phase gamePhase = Phase.Main1, Step step = Step.None)
	{
		AddPlay(AdjustTurnToGlobalTurn(whoseTurn, turn), gamePhase, step, new TryTOCAction(TOC_grpid));
	}

	protected void AI_DontAttack(uint turn)
	{
		_turnsToNotAttack.Add(AdjustTurnToGlobalTurn(Turn.AI, turn));
	}

	protected void AI_AttackAll(uint turn)
	{
		_turnsToAttackAll.Add(AdjustTurnToGlobalTurn(Turn.AI, turn));
	}

	protected void AI_AttackWithKeyCreature(uint turn, string creature)
	{
		uint key = AdjustTurnToGlobalTurn(Turn.AI, turn);
		if (!_turnsToAttackWithCreature.TryGetValue(key, out var value))
		{
			value = new List<uint>();
			_turnsToAttackWithCreature[key] = value;
		}
		uint item = ConvertANATitleToGRPID(creature);
		value.Add(item);
	}

	protected void AddPlay(uint turn, Phase gamePhase, Step step, ScriptedAction action)
	{
		if (!_requestedPlays.TryGetValue(turn, out var value))
		{
			value = new SortedDictionary<Phase, SortedDictionary<Step, Queue<ScriptedAction>>>();
			_requestedPlays.Add(turn, value);
		}
		if (!value.TryGetValue(gamePhase, out var value2))
		{
			value2 = new SortedDictionary<Step, Queue<ScriptedAction>>();
			value.Add(gamePhase, value2);
		}
		if (!value2.TryGetValue(step, out var value3))
		{
			value3 = new Queue<ScriptedAction>();
			value2.Add(step, value3);
		}
		value3.Enqueue(action);
	}

	protected void EnqueueASAPAction(string cardname)
	{
		_requestedAsapPlays.Add(ConvertANATitleToGRPID(cardname));
	}

	protected void AddTopPickForPayingCost(string cardname)
	{
		_topPicksForPayingCosts.Add(ConvertANATitleToGRPID(cardname));
	}

	protected void AddTopPickForChooseAction(string cardname)
	{
		_topPicksChooseActions.Add(ConvertANATitleToGRPID(cardname));
	}

	protected void SetAIsPreferredTarget(string targeter, params string[] targetTitle)
	{
		if (!_targetPreferences.TryGetValue(ConvertANATitleToGRPID(targeter), out var value))
		{
			value = new TargetingPreferences();
			_targetPreferences[ConvertANATitleToGRPID(targeter)] = value;
		}
		List<uint> list = new List<uint>();
		foreach (string englishCardTitle in targetTitle)
		{
			list.Add(ConvertANATitleToGRPID(englishCardTitle));
		}
		value.PreferredTargets.AddRange(list);
	}

	protected void AIShouldChumpWith(string creature)
	{
		_creaturesToChumpWith.Add(ConvertANATitleToGRPID(creature));
	}

	protected void SetAIsPreferredTarget(string targeter, params TargetCharacteristics[] targetChars)
	{
		if (!_targetPreferences.TryGetValue(ConvertANATitleToGRPID(targeter), out var value))
		{
			value = new TargetingPreferences();
			_targetPreferences[ConvertANATitleToGRPID(targeter)] = value;
		}
		value.PreferredCharacteristics.AddRange(targetChars);
	}

	private static uint ConvertANATitleToGRPID(string englishCardTitle)
	{
		if (EnglishTitleToGRPIDLookup.TryGetValue(englishCardTitle, out var value))
		{
			return value;
		}
		return 0u;
	}

	private static uint[] ConvertANATitleToGRPID(params string[] englishCardTitles)
	{
		List<uint> list = new List<uint>();
		foreach (string englishCardTitle in englishCardTitles)
		{
			list.Add(ConvertANATitleToGRPID(englishCardTitle));
		}
		return list.ToArray();
	}

	protected string CheckLocTextDeviceType(string BaseKey, string HandheldKey = null)
	{
		if (PlatformUtils.IsHandheld() && HandheldKey != null)
		{
			return HandheldKey;
		}
		return BaseKey;
	}
}
