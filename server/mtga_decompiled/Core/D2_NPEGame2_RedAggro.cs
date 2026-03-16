using Wotc.Mtgo.Gre.External.Messaging;

public class D2_NPEGame2_RedAggro : NPE_Game
{
	protected override void AddGameContent()
	{
		SetBattlefield("GRN");
		SetStartingPlayer(Turn.AI);
		SetOpponentPortrait(NPEController.Actor.Goblin);
		AddOnAIDefeatedDialog(2f, "NPE/Game02/End/GoblinDefeat/Miirbo_14", OpponentPortrait, WwiseEvents.vo_goblin_sep_61_v4.EventName);
		AddActionReminder(ActionReminderType.PlayALand, "NPE/Game01/Turn03/ActionReminder_0", 4f, 3f, 1u);
		AddActionReminder(ActionReminderType.PlayALand, "NPE/Game01/Turn03/ActionReminder_0", 7f, 5f, 2u, 3u, 4u);
		AddInterceptorDialog(InterceptionType.PassingWithLandNotPlayed, "NPE/Game01/Turn01/Interceptor_1", Phase.Main2, Step.None, Turn.Human, 1u, 2u, 3u, 4u, 5u, 6u);
		AddActionReminder(ActionReminderType.CastACreature, "NPE/Game02/Turn01/ActionReminder_33", 5f, 3f, 1u, 2u, 3u, 4u, 5u, 6u);
		AddInterceptorDialog(InterceptionType.NotAttackingWithAll, "NPE/Game01/Turn06/Interceptor_31", Phase.Combat, Step.DeclareAttack, Turn.Human, 4u);
		AddInterceptorDialog(InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main2, Step.None, Turn.Human, 3u);
		AddAlwaysOnInterceptorDialog(InterceptionType.BadTargetting, "NPE/Game01/Turn00/AlwaysReminder_4", "NPE/Game01/Turn00/AlwaysReminder_5", "NPE/Game02/Turn00/AlwaysReminder_35");
		AddAlwaysOnInterceptorDialog(InterceptionType.CantAffordSpell, "NPE/Game01/Turn00/AlwaysReminder_2", "NPE/Game01/Turn00/AlwaysReminder_3");
		AI_AttackAll(1u);
		AI_AttackAll(2u);
		AI_AttackAll(3u);
		AI_AttackAll(4u);
		AI_AttackAll(5u);
		AI_AttackAll(6u);
		AI_AttackAll(7u);
		AI_AttackAll(8u);
		AddTopPickForPayingCost("Raging Goblin");
		PlayDialog(2f, "NPE/Game02/Turn01/Miirbo_01", OpponentPortrait, WwiseEvents.vo_goblin_sep_36.EventName, Turn.AI, 1u);
		AI_Casts(Turn.AI, 1u, "Raging Goblin");
		TriggerableShowHangerOnBattlefield(4f, new HangerSituation
		{
			UseNPEHanger = true,
			HideSummoningSickness = true
		}, Turn.AI, 1u, "Raging Goblin", TriggerCondition.EntersBattlefield);
		PlayDialog(2.7f, "NPE/Game02/Turn01/Miirbo_02", OpponentPortrait, WwiseEvents.vo_goblin_sep_38.EventName, Turn.AI, 1u, Phase.Main2);
		DoPause(0.1f, Turn.AI, 1u, Phase.Main2);
		PlayDialog(1f, "NPE/Game02/Turn01/Goblin_01", NPEController.Actor.Token, WwiseEvents.vo_goblin_35_b.EventName, Turn.AI, 1u, Phase.Main2);
		DoPause(0.4f, Turn.AI, 1u, Phase.Main2);
		PlayDialog(4.5f, "NPE/Game02/Turn01/Sparky_02", SparkyPortrait, WwiseEvents.vo_sparky_sep_40.EventName, Turn.AI, 1u, Phase.Main2);
		AI_Casts(Turn.AI, 2u, "Raging Goblin");
		AI_Casts(Turn.AI, 2u, "Raging Goblin");
		DoPause(1f, Turn.AI, 2u, Phase.Combat, Step.BeginCombat);
		PlayDialog(3.3f, "NPE/Game02/Turn02/Sparky_03", SparkyPortrait, WwiseEvents.vo_sparky_sep_41.EventName, Turn.AI, 2u, Phase.Combat, Step.BeginCombat);
		PlayDialog(3.3f, "NPE/Game02/Turn02/Miirbo_03", OpponentPortrait, WwiseEvents.vo_goblin_sep_42.EventName, Turn.AI, 2u, Phase.Main2);
		DoPause(0.2f, Turn.AI, 2u, Phase.Main2);
		PlayDialog(2f, "NPE/Game02/Turn02/Goblin_02", NPEController.Actor.Token, WwiseEvents.vo_goblin_39_b.EventName, Turn.AI, 2u, Phase.Main2, Step.None, followCard: true);
		PlayDialog(3f, "NPE/Game02/Turn02/Sparky_04", SparkyPortrait, WwiseEvents.vo_sparky_sep_44.EventName, Turn.Human, 2u);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn02/Interceptor_8", Phase.Main2, Step.None, Turn.Human, 2u);
		AI_Casts(Turn.AI, 3u, "Goblin Bruiser", Phase.Main2);
		TriggerablePause(2.5f, Turn.Human, 1u, "Goblin Bruiser", TriggerCondition.EntersBattlefield);
		DoPause(1f, Turn.AI, 3u, Phase.Ending, Step.End);
		PlayDialog(3.5f, "NPE/Game02/Turn03/Miirbo_04", OpponentPortrait, WwiseEvents.vo_goblin_sep_45.EventName, Turn.AI, 3u, Phase.Ending, Step.End);
		DoPause(0.5f, Turn.AI, 3u, Phase.Ending, Step.End);
		ShowTooltipBumper(5f, "NPE/Game02/Turn03/CenterPrompt_36", PromptType.Center, Turn.AI, 3u, Phase.Ending, Step.End);
		DoPause(1.2f, Turn.AI, 3u, Phase.Ending, Step.End);
		AddInterceptorDialogForKeyCard("Raging Goblin", InterceptionType.MissingKeyBlock, "NPE/Game02/Turn03/Interceptor_37", Phase.Combat, Step.DeclareBlock, Turn.AI, 3u);
		AddBlockingDelayedReminder(CheckLocTextDeviceType("NPE/Game04/Turn06/BlockingReminder_59", "NPE/Game04/Turn06/BlockingReminder_59_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn05/BlockingSubmitReminder_24", "NPE/Game01/Turn05/BlockingSubmitReminder_24_Handheld"), 1f, 3f, 3u, "Shrine Keeper");
		DoPause(1.5f, Turn.Human, 3u, Phase.Beginning, Step.Untap);
		PlayDialog(4f, "NPE/Game02/Turn03/Sparky_05", SparkyPortrait, WwiseEvents.vo_sparky_sep_46.EventName, Turn.Human, 3u, Phase.Combat, Step.DeclareAttack);
		PlayDialog(2f, "NPE/Game02/Turn03/Sparky_06", SparkyPortrait, WwiseEvents.vo_sparky_sep_47.EventName, Turn.Human, 3u, Phase.Combat, Step.DeclareAttack);
		AddInterceptorDialogForKeyCard("Sanctuary Cat", InterceptionType.PassingWithCreatureNotCast, "NPE/Game02/Turn03/Interceptor_38", Phase.Main2, Step.None, Turn.Human, 3u);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.PassingWithCreatureNotCast, "NPE/Game02/Turn03/Interceptor_38", Phase.Main2, Step.None, Turn.Human, 3u);
		AddDontAttackReminder("NPE/Game01/Turn07/DontAttackReminder_20", 0.75f, 3f, 3u);
		AddInterceptorDialog(InterceptionType.BadAttack, "NPE/Game01/Turn05/Interceptor_21", Phase.Combat, Step.DeclareAttack, Turn.Human, 3u);
		AI_Casts(Turn.AI, 4u, "Goblin Grenade", Phase.Main2);
		TriggerablePause(4f, Turn.AI, 4u, "Goblin Grenade", TriggerCondition.IsCast);
		SetAIsPreferredTarget("Goblin Grenade", TargetCharacteristics.IsTheHumanPlayer);
		PlayDialog(2.8f, "NPE/Game02/Turn04/Miirbo_05", OpponentPortrait, WwiseEvents.vo_goblin_sep_48.EventName, Turn.AI, 4u, Phase.Combat, Step.BeginCombat);
		AddBlockingDelayedReminder(CheckLocTextDeviceType("NPE/Game04/Turn06/BlockingReminder_59", "NPE/Game04/Turn06/BlockingReminder_59_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn05/BlockingSubmitReminder_24", "NPE/Game01/Turn05/BlockingSubmitReminder_24_Handheld"), 4f, 3f, 4u, "Sanctuary Cat", "Shrine Keeper", "Shrine Keeper");
		AddInterceptorDialogForKeyCard("Raging Goblin", InterceptionType.MissingKeyBlock, "NPE/Game02/Turn04/Interceptor_39", Phase.Combat, Step.DeclareBlock, Turn.AI, 4u);
		PlayDialog(4f, "NPE/Game02/Turn04/Miirbo_06", OpponentPortrait, WwiseEvents.vo_goblin_sep_49_v2.EventName, Turn.AI, 4u, Phase.Main2);
		PlayDialog(1.5f, "NPE/Game02/Turn04/Goblin_03", NPEController.Actor.Token, WwiseEvents.vo_gobwaitwha_01.EventName, Turn.AI, 4u, Phase.Main2);
		DoPause(2f, Turn.AI, 4u, Phase.Ending, Step.End);
		AddInterceptorDialogForKeyCard("Sanctuary Cat", InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main2, Step.None, Turn.Human, 4u);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main2, Step.None, Turn.Human, 4u);
		AddAttackingReminder(CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingReminder_12", "NPE/Game01/Turn03/AttackingReminder_12_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingSubmitReminder_13", "NPE/Game01/Turn03/AttackingSubmitReminder_13_Handheld"), 2f, 3f, 4u);
		PlayDialog(1f, "NPE/Game02/Turn04/Miirbo_07", OpponentPortrait, WwiseEvents.vo_goblin_sep_51.EventName, Turn.Human, 4u, Phase.Main2);
		AI_Casts(Turn.AI, 5u, "Goblin Gang Leader", Phase.Main2);
		ShowTriggerableTooltipBumper(4f, "NPE/Game02/Turn05/CenterPrompt_40", PromptType.Center, Turn.AI, 5u, "Goblin Gang Leader", TriggerCondition.EntersBattlefield);
		TriggerablePause(1.5f, Turn.AI, 5u, "Goblin Gang Leader", TriggerCondition.AbilityTriggersOrActivates);
		PlayDialog(4.4f, "NPE/Game02/Turn05/Miirbo_08", OpponentPortrait, WwiseEvents.vo_goblin_sep_52.EventName, Turn.AI, 5u, Phase.Main2);
		DoPause(1f, Turn.AI, 5u, Phase.Main2);
		PlayDialog(2.5f, "NPE/Game02/Turn05/Miirbo_09", OpponentPortrait, WwiseEvents.vo_goblin_sep_53.EventName, Turn.AI, 5u, Phase.Main2);
		ShowTriggerableTooltipBumper(4.5f, "NPE/Game02/Turn05/CenterPrompt_41", PromptType.Center, Turn.AI, 5u, "Spiritual Guardian", TriggerCondition.EntersBattlefield);
		AddInterceptorDialogForKeyCard("Spiritual Guardian", InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main2, Step.None, Turn.Human, 5u);
		TriggerablePause(3f, Turn.Human, 5u, "Spiritual Guardian", TriggerCondition.AbilityTriggersOrActivates);
		PlayTriggerableDialog(3.5f, "NPE/Game02/Turn05/Sparky_07", SparkyPortrait, WwiseEvents.vo_sparky_sep_54.EventName, Turn.Human, 5u, "Spiritual Guardian", TriggerCondition.EntersBattlefield);
		AI_Casts(Turn.AI, 6u, "Ogre Painbringer", Phase.Main2);
		PlayDialog(5.5f, "NPE/Game02/Turn06/Miirbo_10", OpponentPortrait, WwiseEvents.vo_goblin_sep_55.EventName, Turn.AI, 6u, Phase.Combat, Step.DeclareAttack);
		AddBlockingDelayedReminder(CheckLocTextDeviceType("NPE/Game04/Turn06/BlockingReminder_59", "NPE/Game04/Turn06/BlockingReminder_59_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn05/BlockingSubmitReminder_24", "NPE/Game01/Turn05/BlockingSubmitReminder_24_Handheld"), 4f, 3f, 6u, "Spiritual Guardian");
		PlayDialog(4f, "NPE/Game02/Turn06/Miirbo_11", OpponentPortrait, WwiseEvents.vo_goblin_sep_56.EventName, Turn.AI, 6u, Phase.Main2);
		TriggerablePause(1.5f, Turn.AI, 6u, "Ogre Painbringer", TriggerCondition.IsCast);
		PlayTriggerableDialog(3.2f, "NPE/Game02/Turn06/Sparky_08", SparkyPortrait, WwiseEvents.vo_sparky_sep_58.EventName, Turn.AI, 6u, "Ogre Painbringer", TriggerCondition.IsCast);
		TriggerablePause(2.5f, Turn.AI, 6u, "Ogre Painbringer", TriggerCondition.AbilityTriggersOrActivates);
		PlayDialog(5.5f, "NPE/Game02/Turn07/Miirbo_12", OpponentPortrait, WwiseEvents.vo_goblin_sep_59_v2.EventName, Turn.AI, 7u, Phase.Main2);
		PlayDialog(3f, "NPE/Game02/Turn07/Miirbo_13", OpponentPortrait, WwiseEvents.vo_goblin_sep_60.EventName, Turn.Human, 7u, Phase.Combat, Step.BeginCombat);
		AI_Casts(Turn.AI, 8u, "Raging Goblin");
		PlayTriggerableDialog(3f, "NPE/Game02/Turn08/Miirbo_15", OpponentPortrait, WwiseEvents.vo_goblin_57.EventName, Turn.AI, 8u, "Raging Goblin", TriggerCondition.EntersBattlefield);
		PlayDialog(2.5f, "NPE/Game02/Turn08/Sparky_09", SparkyPortrait, WwiseEvents.vo_sparky_058_v1.EventName, Turn.AI, 8u, Phase.Combat, Step.DeclareAttack);
		PlayDialog(2.6f, "NPE/Game02/Turn08/Miirbo_16", OpponentPortrait, WwiseEvents.vo_goblin_59.EventName, Turn.AI, 8u, Phase.Main2);
		PlayDialog(3.6f, "NPE/Game02/Turn09/Miirbo_17", OpponentPortrait, WwiseEvents.vo_goblin_60.EventName, Turn.AI, 9u, Phase.Main2);
		PlayDialog(3.5f, "NPE/Game02/Turn09/Sparky_10", SparkyPortrait, WwiseEvents.vo_sparky_061_c_v1.EventName, Turn.AI, 9u, Phase.Main2);
		AddInterceptorDialog(InterceptionType.NotAttackingWithAll, "NPE/Game01/Turn06/Interceptor_31", Phase.Combat, Step.DeclareAttack, Turn.Human, 9u, 10u);
	}
}
