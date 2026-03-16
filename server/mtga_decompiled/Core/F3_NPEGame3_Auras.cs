using Wotc.Mtgo.Gre.External.Messaging;

public class F3_NPEGame3_Auras : NPE_Game
{
	protected override void AddGameContent()
	{
		SetBattlefield("XLN");
		SetStartingPlayer(Turn.AI);
		SetOpponentPortrait(NPEController.Actor.Merfolk);
		AddOnAIDefeatedDialog(3.5f, "NPE/Game03/End/MerfolkDefeat/Calubi_16", OpponentPortrait, WwiseEvents.vo_merfolk_sep_90.EventName);
		AddOnAIVictoryDialog(4.5f, "NPE/Game03/End/MerfolkVictory/Calubi_17", OpponentPortrait, WwiseEvents.vo_merfolk_sep_80.EventName);
		AIShouldChumpWith("Shorecomber Crab");
		AddActionReminder(ActionReminderType.PlayALand, "NPE/Game01/Turn03/ActionReminder_0", 7f, 5f, 1u);
		AddInterceptorDialog(InterceptionType.PassingWithLandNotPlayed, "NPE/Game01/Turn01/Interceptor_1", Phase.Main2, Step.None, Turn.Human, 1u, 3u, 4u, 5u);
		AddInterceptorDialog(InterceptionType.PassingWithLandNotPlayed, "NPE/Game01/Turn01/Interceptor_1", Phase.Main1, Step.None, Turn.Human, 2u);
		AddActionReminder(ActionReminderType.CastACreature, "NPE/Game02/Turn01/ActionReminder_33", 4f, 3f, 1u, 2u, 3u, 5u);
		AddAlwaysOnInterceptorDialog(InterceptionType.BadTargetting, "NPE/Game01/Turn00/AlwaysReminder_4", "NPE/Game01/Turn00/AlwaysReminder_5", "NPE/Game02/Turn00/AlwaysReminder_35");
		AddAlwaysOnInterceptorDialog(InterceptionType.BadAuraTargetting, "NPE/Game03/Turn00/AlwaysReminder_42");
		AddAlwaysOnInterceptorDialog(InterceptionType.CantAffordSpell, "NPE/Game01/Turn00/AlwaysReminder_2", "NPE/Game01/Turn00/AlwaysReminder_3");
		AI_Casts(Turn.AI, 1u, "Zephyr Gull");
		DoPause(0.5f, Turn.AI, 1u);
		PlayDialog(5f, "NPE/Game03/Intro/Sparky_01", SparkyPortrait, WwiseEvents.vo_sparky_sep_64.EventName, Turn.AI, 1u);
		PlayDialog(3.5f, "NPE/Game03/Intro/Sparky_02", SparkyPortrait, WwiseEvents.vo_sparky_sep_65.EventName, Turn.AI, 1u);
		PlayDialog(3.5f, "NPE/Game03/Intro/Calubi_01", OpponentPortrait, WwiseEvents.vo_merfolk_sep_66.EventName, Turn.AI, 1u);
		TriggerablePause(0.5f, Turn.AI, 1u, "Zephyr Gull", TriggerCondition.EntersBattlefield);
		TriggerableShowHangerOnBattlefield(4f, new HangerSituation
		{
			UseNPEHanger = true,
			HideSummoningSickness = true
		}, Turn.AI, 1u, "Zephyr Gull", TriggerCondition.EntersBattlefield);
		PlayDialog(4f, "NPE/Game03/Turn01/Calubi_03", OpponentPortrait, WwiseEvents.vo_merfolk_sep_67.EventName, Turn.AI, 1u, Phase.Ending, Step.End);
		AddInterceptorDialogForKeyCard("Sanctuary Cat", InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn02/Interceptor_8", Phase.Main2, Step.None, Turn.Human, 1u);
		AI_Casts(Turn.AI, 2u, "River's Favor");
		SetAIsPreferredTarget("River's Favor", "Zephyr Gull");
		ShowHangerOnBattlefield(4f, new HangerSituation
		{
			UseNPEHanger = true,
			HideTapped = true
		}, "Zephyr Gull", Turn.AI, 2u, Phase.Combat, Step.DeclareBlock);
		PlayTriggerableDialog(5f, "NPE/Game03/Turn02/Calubi_04", OpponentPortrait, WwiseEvents.vo_merfolk_sep_68.EventName, Turn.AI, 2u, "River's Favor", TriggerCondition.IsCast);
		PlayDialog(4.5f, "NPE/Game03/Turn02/Sparky_01", SparkyPortrait, WwiseEvents.vo_sparky_sep_69.EventName, Turn.AI, 2u, Phase.Combat, Step.DeclareAttack);
		TriggerablePause(3f, Turn.AI, 2u, "River's Favor", TriggerCondition.IsCast);
		ShowTriggerableTooltipBumper(4f, "NPE/Game03/Turn02/CenterPrompt_43", PromptType.Center, Turn.AI, 2u, "River's Favor", TriggerCondition.EntersBattlefield);
		ShowTriggerableTooltipBumper(4.5f, "NPE/Game03/Turn02/CenterPrompt_44", PromptType.Center, Turn.Human, 2u, "Knight's Pledge", TriggerCondition.IsDrawn);
		PlayDialog(3.5f, "NPE/Game03/Turn02/Sparky_02", SparkyPortrait, WwiseEvents.vo_sparky_sep_71.EventName, Turn.Human, 2u);
		TriggerablePause(0.5f, Turn.Human, 2u, "Knight's Pledge", TriggerCondition.EntersBattlefield);
		PlayTriggerableDialog(4f, "NPE/Game03/Turn02/Calubi_06", OpponentPortrait, WwiseEvents.vo_merfolk_sep_72.EventName, Turn.Human, 2u, "Knight's Pledge", TriggerCondition.EntersBattlefield);
		AddTargetReminder("NPE/Game03/Turn02/TargetReminder_45", 0.5f, 2f, Turn.Human, 2u, "Sanctuary Cat");
		AddActionReminder(ActionReminderType.CastASpell, "NPE/Game03/Turn02/ActionReminder_46", 0.5f, 3f, 2u, 4u, 6u);
		AddInterceptorDialogForKeyCard("Knight's Pledge", InterceptionType.PassingWithSpellNotCast, "NPE/Game03/Turn02/Interceptor_47", Phase.Main1, Step.None, Turn.Human, 2u, 4u);
		AddAttackingReminder(CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingReminder_12", "NPE/Game01/Turn03/AttackingReminder_12_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingSubmitReminder_13", "NPE/Game01/Turn03/AttackingSubmitReminder_13_Handheld"), 2f, 3f, 2u);
		AddInterceptorDialogForKeyCard("Sanctuary Cat", InterceptionType.MissingKeyAttacker, "NPE/Game01/Turn03/Interceptor_14", Phase.Combat, Step.DeclareAttack, Turn.Human, 2u);
		AI_Casts(Turn.AI, 3u, "Shorecomber Crab", Phase.Main2);
		PlayTriggerableDialog(6.5f, "NPE/Game03/Turn03/Calubi_07", OpponentPortrait, WwiseEvents.vo_merfolk_sep_73.EventName, Turn.AI, 3u, "Shorecomber Crab", TriggerCondition.EntersBattlefield);
		PlayDialog(3.7f, "NPE/Game03/Turn02/g3_t2_144", SparkyPortrait, "vo_sparky_jan19_tutoriallines_g3_t2_144", Turn.AI, 3u, Phase.Ending, Step.End);
		PlayTriggerableDialog(4.2f, "NPE/Game03/Turn03/Calubi_08", OpponentPortrait, WwiseEvents.vo_merfolk_sep_76.EventName, Turn.Human, 3u, "Shorecomber Crab", TriggerCondition.IsDealtDamage);
		AddInterceptorDialogForKeyCard("Loxodon Line Breaker", InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main2, Step.None, Turn.Human, 3u);
		AI_Casts(Turn.AI, 4u, "Waterknot", Phase.Main2);
		SetAIsPreferredTarget("Waterknot", TargetCharacteristics.HumanControls);
		SetAIsPreferredTarget("Waterknot", "Loxodon Line Breaker");
		TriggerablePause(4.5f, Turn.AI, 4u, "Waterknot", TriggerCondition.IsCast);
		PlayDialog(3f, "NPE/Game03/Turn04/Calubi_09", OpponentPortrait, WwiseEvents.vo_merfolk_sep_77_v2.EventName, Turn.AI, 4u, Phase.Main2);
		PlayDialog(3f, "NPE/Game03/Turn04/g3_t4_145", SparkyPortrait, "vo_sparky_jan19_tutoriallines_g3_t4_145", Turn.Human, 4u);
		AddTargetReminder("NPE/Game03/Turn04/TargetReminder_49", 2f, 3f, Turn.Human, 4u, "Sanctuary Cat");
		AI_Casts(Turn.AI, 5u, "Divination", Phase.Main2);
		AI_Casts(Turn.AI, 5u, "Shorecomber Crab", Phase.Main2);
		PlayDialog(5.5f, "NPE/Game03/Turn05/Calubi_11", OpponentPortrait, WwiseEvents.vo_merfolk_sep_79.EventName, Turn.AI, 5u, Phase.Main2);
		PlayDialog(3f, "NPE/Game03/Turn05/Sparky_05", SparkyPortrait, WwiseEvents.vo_sparky_sep_80.EventName, Turn.AI, 5u, Phase.Main2);
		AI_Casts(Turn.AI, 6u, "Titanic Pelagosaur", Phase.Main2);
		AI_AttackWithKeyCreature(6u, "Zephyr Gull");
		TriggerablePause(1f, Turn.AI, 6u, "Titanic Pelagosaur", TriggerCondition.EntersBattlefield);
		PlayTriggerableDialog(7f, "NPE/Game03/Turn06/Calubi_12", OpponentPortrait, WwiseEvents.vo_merfolk_sep_81.EventName, Turn.AI, 6u, "Titanic Pelagosaur", TriggerCondition.EntersBattlefield);
		TriggerablePause(0.5f, Turn.AI, 6u, "Titanic Pelagosaur", TriggerCondition.EntersBattlefield);
		PlayTriggerableDialog(3f, "NPE/Game03/Turn06/Sparky_06", SparkyPortrait, WwiseEvents.vo_sparky_sep_82.EventName, Turn.AI, 6u, "Titanic Pelagosaur", TriggerCondition.EntersBattlefield);
		AddInterceptorDialogForKeyCard("Spiritual Guardian", InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main2, Step.None, Turn.Human, 5u);
		AddTargetReminder(CheckLocTextDeviceType("NPE/Extra/Extra09", "NPE/Extra/Extra09_Handheld"), 1f, 5f, Turn.Human, 6u, "Sanctuary Cat");
		AddInterceptorDialogForKeyCard("Angelic Reward", InterceptionType.PassingWithSpellNotCast, "NPE/Game03/Turn02/Interceptor_47", Phase.Main1, Step.None, Turn.Human, 6u);
		AddInterceptorDialogForKeyCard("HAS_FLYING", InterceptionType.MissingKeyAttacker, "NPE/Game03/Turn06/Interceptor_51", Phase.Combat, Step.DeclareAttack, Turn.Human, 6u);
		AddAttackingReminder(CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingReminder_12", "NPE/Game01/Turn03/AttackingReminder_12_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingSubmitReminder_13", "NPE/Game01/Turn03/AttackingSubmitReminder_13_Handheld"), 2f, 3f, 6u, "HAS_FLYING");
		AI_AttackWithKeyCreature(7u, "Titanic Pelagosaur");
		AI_AttackWithKeyCreature(7u, "Zephyr Gull");
		AI_Casts(Turn.AI, 7u, "Titanic Pelagosaur", Phase.Main2);
		PlayDialog(9f, "NPE/Game03/Turn07/Calubi_13", OpponentPortrait, WwiseEvents.vo_merfolk_sep_83.EventName, Turn.AI, 7u, Phase.Main2);
		PlayDialog(3.5f, "NPE/Game03/Turn07/Sparky_07", SparkyPortrait, WwiseEvents.vo_sparky_sep_84.EventName, Turn.AI, 7u, Phase.Main2);
		PlayDialog(5.5f, "NPE/Game03/Turn07/Sparky_08", SparkyPortrait, WwiseEvents.vo_sparky_sep_85.EventName, Turn.Human, 7u);
		PlayDialog(3.5f, "NPE/Game03/Turn07/Sparky_08b", SparkyPortrait, WwiseEvents.vo_sparky_sep_86.EventName, Turn.Human, 7u);
		DoPause(1.5f, Turn.Human, 7u);
		PlayDialog(2f, "NPE/Game03/Turn07/Calubi_14", OpponentPortrait, WwiseEvents.vo_merfolk_sep_87.EventName, Turn.Human, 7u);
		PlayDialog(4f, "NPE/Game03/Turn07/Calubi_15", OpponentPortrait, WwiseEvents.vo_merfolk_sep_88.EventName, Turn.Human, 7u);
		PlayDialog(3.5f, "NPE/Game03/Turn07/Sparky_09", SparkyPortrait, WwiseEvents.vo_sparky_sep_89.EventName, Turn.Human, 7u);
		AI_AttackAll(8u);
		AI_AttackAll(9u);
		AddInterceptorDialog(InterceptionType.NotAttackingWithAll, "NPE/Game01/Turn06/Interceptor_31", Phase.Combat, Step.DeclareAttack, Turn.Human, 9u, 10u);
	}
}
