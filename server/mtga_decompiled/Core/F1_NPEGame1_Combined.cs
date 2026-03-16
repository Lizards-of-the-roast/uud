using Wotc.Mtgo.Gre.External.Messaging;

public class F1_NPEGame1_Combined : NPE_Game
{
	protected override void AddGameContent()
	{
		SetBattlefield("DAR");
		ShowCinematicAtStart = true;
		SetStartingPlayer(Turn.Human);
		SetOpponentPortrait(NPEController.Actor.Elf);
		AddOnAIDefeatedDialog(2.5f, "NPE/Game01/End/ElfDefeat/Kylea_09", OpponentPortrait, WwiseEvents.vo_elf_sep_31.EventName);
		AddActionReminder(ActionReminderType.PlayALand, "NPE/Game01/Turn03/ActionReminder_0", 6f, 5f, 3u, 4u, 5u, 6u);
		AddInterceptorDialog(InterceptionType.PassingWithLandNotPlayed, "NPE/Game01/Turn01/Interceptor_1", Phase.Main2, Step.None, Turn.Human, 1u, 2u, 3u);
		AddAlwaysOnInterceptorDialog(InterceptionType.CantAffordSpell, "NPE/Game01/Turn00/AlwaysReminder_2", "NPE/Game01/Turn00/AlwaysReminder_3");
		AddAlwaysOnInterceptorDialog(InterceptionType.BadTargetting, "NPE/Game01/Turn00/AlwaysReminder_4", "NPE/Game01/Turn00/AlwaysReminder_5");
		PlayDialog(3f, "NPE/Intro/Sparky_01", SparkyPortrait, WwiseEvents.vo_sparky_sep_01.EventName, Turn.Human, 1u, Phase.Beginning, Step.Upkeep);
		PlayDialog(8f, "NPE/Intro/Sparky_03", SparkyPortrait, WwiseEvents.vo_sparky_sep_03.EventName, Turn.Human, 1u, Phase.Beginning, Step.Upkeep);
		PlayFallTriggeringDialog(3f, "NPE/Intro/Sparky_04", SparkyPortrait, WwiseEvents.vo_sparky_sep_04.EventName, Turn.Human, 1u, Phase.Beginning, Step.Upkeep);
		ShowDismissableDeluxeTooltip(DeluxeTooltipType.DominariaFall, Turn.Human, 1u, Phase.Beginning, Step.Upkeep);
		PlayDialog(6f, "NPE/Game01/Intro/Sparky_03", SparkyPortrait, WwiseEvents.vo_sparky_sep_07.EventName, Turn.Human, 1u);
		PlayDialog(3f, "NPE/Game01/Turn01/Kylea_03", OpponentPortrait, WwiseEvents.vo_elf_sep_15.EventName, Turn.Human, 1u);
		PlayDialog(4.75f, "NPE/Game01/Intro/Sparky_04", SparkyPortrait, WwiseEvents.vo_sparky_sep_10.EventName, Turn.Human, 1u);
		DoPause(0.5f, Turn.Human, 1u);
		PlayDialog(6.2f, "NPE/Game01/Turn01/Sparky_05", SparkyPortrait, WwiseEvents.vo_sparky_sep_11.EventName, Turn.Human, 1u);
		AddActionReminder(ActionReminderType.PlayALand, "NPE/Game01/Turn01/ActionReminder_6", 0.5f, 3f, 1u);
		PlayTriggerableDialog(1f, "NPE/Game01/Turn01/Sparky_06", SparkyPortrait, WwiseEvents.vo_sparky_sep_12.EventName, Turn.Human, 1u, "Plains", TriggerCondition.EntersBattlefield);
		DoPause(0.7f, Turn.Human, 1u, Phase.Ending, Step.End);
		PlayDialog(4f, "NPE/Game01/Turn01/Sparky_07", SparkyPortrait, WwiseEvents.vo_sparky_sep_13.EventName, Turn.Human, 1u, Phase.Ending, Step.End);
		PlayDialog(4f, "NPE/Game01/Turn01/Sparky_08", SparkyPortrait, WwiseEvents.vo_sparky_sep_14.EventName, Turn.Human, 1u, Phase.Ending, Step.End);
		DoPause(1f, Turn.Human, 1u, Phase.Ending, Step.End);
		DoPause(1f, Turn.AI, 1u, Phase.Main2);
		PlayDialog(4.5f, "NPE/Game01/Turn02/Sparky_09", SparkyPortrait, WwiseEvents.vo_sparky_sep_16.EventName, Turn.Human, 2u);
		AddActionReminder(ActionReminderType.PlayALand, "NPE/Game01/Turn02/ActionReminder_7", 0.5f, 5f, 2u);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn02/Interceptor_8", Phase.Main2, Step.None, Turn.Human, 2u);
		TriggerDismissableDeluxeTooltip(DeluxeTooltipType.Mana, Turn.Human, 2u, "Plains", TriggerCondition.EntersBattlefield);
		TriggerablePause(1.5f, Turn.Human, 2u, "Shrine Keeper", TriggerCondition.IsCast);
		PlayTriggerableDialog(3.5f, "NPE/Game01/Turn02/Sparky_10", SparkyPortrait, WwiseEvents.vo_sparky_sep_17.EventName, Turn.Human, 2u, "Shrine Keeper", TriggerCondition.EntersBattlefield);
		TriggerablePause(1f, Turn.Human, 2u, "Shrine Keeper", TriggerCondition.EntersBattlefield);
		DoPause(1f, Turn.Human, 2u, Phase.Ending, Step.End);
		AddActionReminder(ActionReminderType.CastACreature, "NPE/Game02/Turn01/ActionReminder_33", 3f, 4f, 2u);
		ShowTooltipBumper(3f, "NPE/Game01/Turn03/CenterPrompt_10", PromptType.Center, Turn.Human, 3u);
		DoPause(3f, Turn.Human, 3u, Phase.Main1, Step.End);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main1, Step.None, Turn.Human, 3u);
		AddActionReminder(ActionReminderType.CastACreature, "NPE/Game02/Turn01/ActionReminder_33", 3f, 4f, 3u);
		PlayDialog(3.7f, "NPE/Game01/Turn03/Sparky_12", SparkyPortrait, WwiseEvents.vo_sparky_sep_19.EventName, Turn.Human, 3u, Phase.Combat, Step.BeginCombat);
		ShowHangerOnBattlefield(4.5f, new HangerSituation
		{
			UseNPEHanger = true
		}, "Shrine Keeper", Turn.Human, 3u, Phase.Combat, Step.BeginCombat, showOnLeftSide: true);
		TriggerablePTExplainer(3f, showPower: true, "Shrine Keeper", Turn.Human, 3u);
		TriggerablePTExplainer(3f, showPower: false, "Shrine Keeper", Turn.Human, 3u);
		TriggerablePause(1.5f, Turn.Human, 3u, "Shrine Keeper", TriggerCondition.SubmittedForAttack);
		AddAttackingReminder(CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingReminder_12", "NPE/Game01/Turn03/AttackingReminder_12_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingSubmitReminder_13", "NPE/Game01/Turn03/AttackingSubmitReminder_13_Handheld"), 0.5f, 6f, 3u, "Shrine Keeper");
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.MissingKeyAttacker, "NPE/Game01/Turn03/Interceptor_14", Phase.Combat, Step.DeclareAttack, Turn.Human, 3u);
		AddAttackingReminder(CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingReminder_15", "NPE/Game01/Turn03/AttackingReminder_15_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingSubmitReminder_13", "NPE/Game01/Turn03/AttackingSubmitReminder_13_Handheld"), 0.5f, 6f, 3u);
		ShowHangerOnBattlefield(4f, new HangerSituation
		{
			UseNPEHanger = true,
			ShowOnlyTapped = true
		}, "Shrine Keeper", Turn.Human, 3u, Phase.Main2);
		PlayDialog(2.3f, "NPE/Game01/Turn03/Kylea_04", OpponentPortrait, WwiseEvents.vo_elf_sep_20.EventName, Turn.Human, 3u, Phase.Main2);
		DoPause(1.5f, Turn.Human, 3u, Phase.Main2);
		AI_Casts(Turn.AI, 3u, "Treetop Warden", Phase.Main2);
		TriggerablePause(1.5f, Turn.AI, 3u, "Treetop Warden", TriggerCondition.IsCast);
		PlayTriggerableDialog(4f, CheckLocTextDeviceType("NPE/Game01/g1_142", "NPE/Game01/g1_142_Handheld"), SparkyPortrait, "vo_sparky_jan19_tutoriallines_g1_142", Turn.AI, 3u, "Treetop Warden", TriggerCondition.EntersBattlefield);
		TriggerablePause(2f, Turn.AI, 5u, "Treetop Warden", TriggerCondition.EntersBattlefield);
		DoPause(3f, Turn.AI, 3u, Phase.Ending, Step.End);
		ShowTooltipBumper(3f, "NPE/Game01/Turn04/CenterPrompt_16", PromptType.Center, Turn.Human, 4u);
		PlayDialog(2f, "NPE/Game01/Turn04/Sparky_13", SparkyPortrait, WwiseEvents.vo_sparky_sep_21.EventName, Turn.Human, 4u, Phase.Combat, Step.BeginCombat);
		AddAttackingReminder("NPE/Game01/Turn04/AttackingReminder_17", CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingSubmitReminder_13", "NPE/Game01/Turn03/AttackingSubmitReminder_13_Handheld"), 0.5f, 3f, 4u);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.MissingKeyAttacker, "NPE/Game01/Turn03/Interceptor_14", Phase.Combat, Step.DeclareAttack, Turn.Human, 4u);
		PlayTriggerableDialog(2.9f, "NPE/Game01/Turn03/g1_t3_143", SparkyPortrait, "vo_sparky_jan19_tutoriallines_g1_t3_143", Turn.Human, 4u, "Treetop Warden", TriggerCondition.IsBlocking);
		TriggerDismissableDeluxeTooltip(DeluxeTooltipType.Combat, Turn.Human, 4u, "Treetop Warden", TriggerCondition.IsBlocking);
		DoPause(1f, Turn.Human, 4u, Phase.Combat, Step.CombatDamage);
		PlayDialog(4.4f, "NPE/Game01/Turn04/Kylea_05", OpponentPortrait, WwiseEvents.vo_elf_sep_22.EventName, Turn.Human, 4u, Phase.Main2);
		TriggerablePause(1.5f, Turn.Human, 4u, "Loxodon Line Breaker", TriggerCondition.EntersBattlefield);
		AI_Casts(Turn.AI, 4u, "Rumbling Baloth", Phase.Main2);
		TriggerablePause(2.5f, Turn.AI, 4u, "Rumbling Baloth", TriggerCondition.IsCast);
		TriggerablePause(1f, Turn.AI, 4u, "Rumbling Baloth", TriggerCondition.EntersBattlefield);
		PlayTriggerableDialog(3f, "NPE/Game01/Turn04/Sparky_14", SparkyPortrait, WwiseEvents.vo_sparky_sep_23.EventName, Turn.AI, 4u, "Rumbling Baloth", TriggerCondition.EntersBattlefield);
		PlayDialog(4.5f, "NPE/Game01/Turn05/Sparky_15", SparkyPortrait, WwiseEvents.vo_sparky_sep_24.EventName, Turn.Human, 5u, Phase.Combat, Step.DeclareAttack);
		AddDontAttackReminder("NPE/Game01/Turn07/DontAttackReminder_20", 0.5f, 3f, 7u);
		AddInterceptorDialog(InterceptionType.BadAttack, "NPE/Game01/Turn05/Interceptor_21", Phase.Combat, Step.DeclareAttack, Turn.Human, 5u);
		AddInterceptorDialogForKeyCard("Loxodon Line Breaker", InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main2, Step.None, Turn.Human, 5u);
		AI_Casts(Turn.AI, 5u, "Feral Roar");
		SetAIsPreferredTarget("Feral Roar", default(TargetCharacteristics));
		SetAIsPreferredTarget("Feral Roar", "Rumbling Baloth");
		TriggerablePause(4f, Turn.AI, 5u, "Feral Roar", TriggerCondition.IsCast);
		ShowTriggerableTooltipBumper(5f, "NPE/Game01/Turn05/CenterPrompt_22", PromptType.Center, Turn.AI, 5u, "Feral Roar", TriggerCondition.SpellFinishesResolving);
		PlayDialog(5f, "NPE/Game01/Turn05/Kylea_06", OpponentPortrait, WwiseEvents.vo_elf_sep_25.EventName, Turn.AI, 5u);
		DoPause(0.5f, Turn.AI, 5u, Phase.Combat, Step.DeclareBlock);
		PlayDialog(3.7f, "NPE/Game01/Turn05/Sparky_16", NPEController.Actor.Spark, WwiseEvents.vo_sparky_sep_26.EventName, Turn.AI, 5u, Phase.Combat, Step.DeclareBlock);
		AddBlockingDelayedReminder("NPE/Game04/Turn06/BlockingReminder_59", CheckLocTextDeviceType("NPE/Game01/Turn05/BlockingSubmitReminder_24", "NPE/Game01/Turn05/BlockingSubmitReminder_24_Handheld"), 1.5f, 3f, 5u, "Shrine Keeper");
		AddInterceptorDialogForKeyCard("Rumbling Baloth", InterceptionType.MissingKeyBlock, "NPE/Game01/Turn05/Interceptor_25", Phase.Combat, Step.DeclareBlock, Turn.AI, 5u);
		AddInterceptorDialogForKeyCard("Rumbling Baloth", InterceptionType.OverBlockingIsBad, "NPE/Game01/Turn05/Interceptor_26", Phase.Combat, Step.DeclareBlock, Turn.AI, 5u);
		ShowTooltipBumper(5.5f, "NPE/Game01/Turn05/CenterPrompt_27", PromptType.Center, Turn.AI, 5u, Phase.Combat, Step.EndCombat);
		PlayTriggerableDialog(2.8f, "NPE/Game01/Turn06/Sparky_17", SparkyPortrait, WwiseEvents.vo_sparky_sep_27.EventName, Turn.Human, 6u, "Take Vengeance", TriggerCondition.IsDrawn);
		AddActionReminder(ActionReminderType.CastASpell, "NPE/Game01/Turn06/ActionReminder_28", 0.5f, 3f, 6u, 7u);
		AddInterceptorDialogForKeyCard("Take Vengeance", InterceptionType.PassingWithSpellNotCast, "NPE/Game01/Turn06/Interceptor_29", Phase.Main2, Step.None, Turn.Human, 6u);
		AddTargetReminder(CheckLocTextDeviceType("NPE/Game01/Turn06/TargetReminder_30", "NPE/Game01/Turn06/TargetReminder_30_Handheld"), 3f, 3f, Turn.Human, 6u, "Rumbling Baloth");
		PlayTriggerableDialog(2f, "NPE/Game01/Turn06/Kylea_07", OpponentPortrait, WwiseEvents.vo_elf_sep_28.EventName, Turn.Human, 6u, "Take Vengeance", TriggerCondition.SpellFinishesResolving);
		AddAttackingReminder(CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingReminder_12", "NPE/Game01/Turn03/AttackingReminder_12_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingSubmitReminder_13", "NPE/Game01/Turn03/AttackingSubmitReminder_13_Handheld"), 2f, 3f, 6u);
		AddInterceptorDialog(InterceptionType.NotAttackingWithAll, "NPE/Game01/Turn06/Interceptor_31", Phase.Combat, Step.DeclareAttack, Turn.Human, 6u, 7u);
		AI_Casts(Turn.AI, 6u, "Treetop Warden");
		AI_Casts(Turn.AI, 6u, "Treetop Warden");
		PlayDialog(2.8f, "NPE/Game01/Turn06/Kylea_08", OpponentPortrait, WwiseEvents.vo_elf_sep_29.EventName, Turn.AI, 6u, Phase.Ending, Step.End);
		PlayDialog(2.7f, "NPE/Game01/Turn07/Sparky_18", NPEController.Actor.Spark, WwiseEvents.vo_sparky_sep_30.EventName, Turn.Human, 7u);
		AddInterceptorDialogForKeyCard("Blinding Radiance", InterceptionType.PassingWithSpellNotCast, "NPE/Game01/Turn07/Interceptor_32", Phase.Main1, Step.None, Turn.Human, 7u);
		ShowHangerOnBattlefield(4.2f, new HangerSituation
		{
			UseNPEHanger = true,
			ShowOnlyTapped = true
		}, "Treetop Warden", Turn.Human, 7u, Phase.Combat, Step.DeclareAttack, showOnLeftSide: true);
		AddAttackingReminder(CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingReminder_12", "NPE/Game01/Turn03/AttackingReminder_12_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn03/AttackingSubmitReminder_13", "NPE/Game01/Turn03/AttackingSubmitReminder_13_Handheld"), 0.5f, 3f, 7u);
		AI_DontAttack(7u);
	}
}
