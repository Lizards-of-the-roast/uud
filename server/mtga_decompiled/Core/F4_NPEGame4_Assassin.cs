using Wotc.Mtgo.Gre.External.Messaging;

public class F4_NPEGame4_Assassin : NPE_Game
{
	protected override void AddGameContent()
	{
		SetBattlefield("ANA");
		SetStartingPlayer(Turn.AI);
		SetOpponentPortrait(NPEController.Actor.Assassin);
		AddOnAIDefeatedDialog(1.5f, "NPE/Game04/End/AssassinDefeat/ViperNang_19", OpponentPortrait, WwiseEvents.vo_assassin_npe_120.EventName);
		AddOnAIVictoryDialog(5f, "NPE/Game04/End/AssassinVictory/ViperNang_20", OpponentPortrait, WwiseEvents.vo_assassin_npe_121.EventName);
		AddTopPickForPayingCost("Nimble Pilferer");
		AIShouldChumpWith("Nimble Pilferer");
		AddInterceptorDialog(InterceptionType.PassingWithLandNotPlayed, "NPE/Game01/Turn01/Interceptor_1", Phase.Main2, Step.None, Turn.Human, 1u, 2u, 3u, 4u, 6u);
		AddActionReminder(ActionReminderType.CastACreature, "NPE/Game02/Turn01/ActionReminder_33", 5f, 3f, 1u, 2u, 3u, 5u, 6u);
		AddInterceptorDialog(InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main2, Step.None, Turn.Human, 1u, 2u, 3u, 5u, 6u);
		AddAlwaysOnInterceptorDialog(InterceptionType.BadTargetting, "NPE/Game01/Turn00/AlwaysReminder_4", "NPE/Game01/Turn00/AlwaysReminder_5", "NPE/Game02/Turn00/AlwaysReminder_35");
		AddAlwaysOnInterceptorDialog(InterceptionType.CantAffordSpell, "NPE/Game01/Turn00/AlwaysReminder_2", "NPE/Game01/Turn00/AlwaysReminder_3");
		PlayDialog(4f, "NPE/Game04/Beginning/Sparky_01", SparkyPortrait, WwiseEvents.vo_sparky_sep_94.EventName, Turn.AI, 1u);
		ShowTooltipBumper(6f, "NPE/Game04/Turn01/CenterPrompt_53", PromptType.Center, Turn.AI, 1u);
		PlayDialog(4f, "NPE/Game04/Beginning/ViperNang_01", OpponentPortrait, WwiseEvents.vo_assassin_npe_95.EventName, Turn.AI, 1u);
		DoPause(0.5f, Turn.AI, 1u, Phase.Ending, Step.End);
		PlayDialog(2.5f, "NPE/Game04/Turn02/ViperNang_03", OpponentPortrait, WwiseEvents.vo_assassin_npe_97.EventName, Turn.AI, 2u, Phase.Main2);
		AI_Casts(Turn.Human, 2u, "Cruel Cut", Phase.Combat, Step.DeclareBlock);
		AI_Casts(Turn.Human, 2u, "Cruel Cut", Phase.Ending, Step.End);
		SetAIsPreferredTarget("Cruel Cut", "Shrine Keeper");
		SetAIsPreferredTarget("Cruel Cut", TargetCharacteristics.IsTapped);
		PlayDialog(2.5f, "NPE/Game04/Turn02/ViperNang_04", OpponentPortrait, WwiseEvents.vo_assassin_npe_98.EventName, Turn.Human, 2u, Phase.Combat, Step.DeclareBlock);
		TriggerablePause(2f, Turn.Human, 2u, "Cruel Cut", TriggerCondition.IsCast);
		ShowTriggerableTooltipBumper(4f, "NPE/Game04/Turn02/CenterPrompt_54", PromptType.Center, Turn.Human, 2u, "Cruel Cut", TriggerCondition.IsCast);
		TriggerablePause(0.5f, Turn.Human, 2u, "Cruel Cut", TriggerCondition.IsCast);
		TriggerablePause(0.5f, Turn.Human, 2u, "Sanctuary Cat", TriggerCondition.Dies);
		PlayTriggerableDialog(3.5f, "NPE/Game04/Turn02/Sparky_02", SparkyPortrait, WwiseEvents.vo_sparky_sep_99.EventName, Turn.Human, 2u, "Sanctuary Cat", TriggerCondition.Dies);
		DoPause(3f, Turn.Human, 2u, Phase.Ending, Step.Cleanup);
		PlayDialog(2f, "NPE/Game04/Turn03/ViperNang_05", OpponentPortrait, WwiseEvents.vo_assassin_npe_100.EventName, Turn.AI, 3u, Phase.Main2);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.MissingKeyAttacker, "NPE/Game01/Turn03/Interceptor_14", Phase.Combat, Step.DeclareAttack, Turn.Human, 3u);
		AI_Casts(Turn.Human, 3u, "Nimble Pilferer", Phase.Combat, Step.DeclareAttack);
		TriggerablePause(1f, Turn.Human, 3u, "Nimble Pilferer", TriggerCondition.IsCast);
		ShowTriggerableTooltipBumper(4f, "NPE/Game04/Turn03/CenterPrompt_55", PromptType.Center, Turn.Human, 3u, "Nimble Pilferer", TriggerCondition.IsCast);
		TriggerableShowHangerOnBattlefield(4f, new HangerSituation
		{
			UseNPEHanger = true,
			HideSummoningSickness = true
		}, Turn.Human, 3u, "Nimble Pilferer", TriggerCondition.IsCast);
		TriggerablePause(1f, Turn.Human, 3u, "Nimble Pilferer", TriggerCondition.IsCast);
		TriggerablePause(0.5f, Turn.Human, 3u, "Nimble Pilferer", TriggerCondition.IsDealtDamage);
		PlayTriggerableDialog(4.8f, "NPE/Game04/Turn03/ViperNang_07", OpponentPortrait, WwiseEvents.vo_assassin_npe_101.EventName, Turn.Human, 3u, "Nimble Pilferer", TriggerCondition.IsDealtDamage);
		AI_AttackAll(4u);
		AddInterceptorDialogForKeyCard("Knight's Pledge", InterceptionType.PassingWithSpellNotCast, "NPE/Game03/Turn02/Interceptor_47", Phase.Main1, Step.None, Turn.Human, 4u);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.MissingKeyAttacker, "NPE/Game01/Turn03/Interceptor_14", Phase.Combat, Step.DeclareAttack, Turn.Human, 4u);
		AI_Casts(Turn.Human, 4u, "Nimble Pilferer", Phase.Combat, Step.DeclareAttack);
		DoPause(1f, Turn.Human, 4u, Phase.Combat, Step.DeclareBlock);
		AI_Casts(Turn.Human, 4u, "Altar's Reap", Phase.Combat, Step.DeclareBlock);
		SetAIsPreferredTarget("Altar's Reap", "Nimble Pilferer");
		TriggerablePause(3.5f, Turn.Human, 4u, "Altar's Reap", TriggerCondition.IsCast);
		PlayDialog(4f, "NPE/Game04/Turn04/Sparky_03", SparkyPortrait, WwiseEvents.vo_sparky_sep_102.EventName, Turn.Human, 4u, Phase.Combat, Step.EndCombat);
		DoPause(0.5f, Turn.Human, 4u, Phase.Combat, Step.EndCombat);
		PlayDialog(3.5f, "NPE/Game04/Turn05/ViperNang_09", OpponentPortrait, WwiseEvents.vo_assassin_npe_104.EventName, Turn.AI, 5u);
		AI_Casts(Turn.AI, 5u, "Soulhunter Rakshasa");
		TriggerablePause(3.5f, Turn.AI, 5u, "Soulhunter Rakshasa", TriggerCondition.AbilityTriggersOrActivates);
		AI_AttackAll(5u);
		ShowTriggerableTooltipBumper(3.5f, "NPE/Game04/Turn05/CenterPrompt_56", PromptType.Center, Turn.Human, 5u, "Tactical Advantage", TriggerCondition.IsDrawn);
		PlayDialog(4.6f, "NPE/Game04/Turn05/Sparky_04", SparkyPortrait, WwiseEvents.vo_sparky_sep_105.EventName, Turn.Human, 5u, Phase.Combat, Step.DeclareAttack);
		AddDontAttackReminder(CheckLocTextDeviceType("NPE/Game04/Turn05/DontAttackReminder_57", "NPE/Game04/Turn05/DontAttackReminder_57_Handheld"), 0.5f, 3f, 5u);
		AddInterceptorDialog(InterceptionType.BadAttack, "NPE/Game04/Turn05/Interceptor_58", Phase.Combat, Step.DeclareAttack, Turn.Human, 5u);
		AI_AttackAll(6u);
		AddBlockingDelayedReminder(CheckLocTextDeviceType("NPE/Game04/Turn06/BlockingReminder_59", "NPE/Game04/Turn06/BlockingReminder_59_Handheld"), CheckLocTextDeviceType("NPE/Game01/Turn05/BlockingSubmitReminder_24", "NPE/Game01/Turn05/BlockingSubmitReminder_24_Handheld"), 5f, 3f, 6u, "Shrine Keeper");
		PlayDialog(4.2f, "NPE/Game04/Turn06/Sparky_05", SparkyPortrait, WwiseEvents.vo_sparky_sep_106.EventName, Turn.AI, 6u, Phase.Combat, Step.DeclareAttack);
		PlayTriggerableDialog(1.5f, "NPE/Game04/Turn06/Sparky_06", SparkyPortrait, WwiseEvents.vo_sparky_sep_107.EventName, Turn.AI, 6u, "Shrine Keeper", TriggerCondition.IsBlocking);
		AddInterceptorDialogForKeyCard("Soulhunter Rakshasa", InterceptionType.MissingKeyBlock, "NPE/Game04/Turn06/Interceptor_61", Phase.Combat, Step.DeclareBlock, Turn.AI, 6u);
		AddActionReminder(ActionReminderType.CastASpell, "NPE/Game04/Turn06/ActionReminder_62", 2f, 3f, Turn.AI, 6u, Phase.Combat, Step.DeclareBlock);
		AddTargetReminder(CheckLocTextDeviceType("NPE/Game04/Turn06/TargetReminder_63", "NPE/Game04/Turn06/TargetReminder_63_Handheld"), 2f, 6f, Turn.AI, 6u, "Shrine Keeper");
		AddInterceptorDialogForKeyCard("Tactical Advantage", InterceptionType.PassingWithSpellNotCast, "NPE/Game04/Turn06/Interceptor_64", Phase.Combat, Step.DeclareBlock, Turn.AI, 6u);
		PlayTriggerableDialog(2f, "NPE/Game04/Turn06/ViperNang_10", OpponentPortrait, WwiseEvents.vo_assassin_npe_108.EventName, Turn.AI, 6u, "Soulhunter Rakshasa", TriggerCondition.Dies);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.MissingKeyAttacker, "NPE/Game01/Turn03/Interceptor_14", Phase.Combat, Step.DeclareAttack, Turn.Human, 6u);
		AI_Casts(Turn.Human, 6u, "Nimble Pilferer", Phase.Combat, Step.DeclareAttack);
		AI_Casts(Turn.Human, 6u, "Nimble Pilferer", Phase.Combat, Step.DeclareAttack);
		AddInterceptorDialogForKeyCard("Tactical Advantage", InterceptionType.PassingWithSpellNotCast, "NPE/Game04/Turn06/Interceptor_65", Phase.Combat, Step.DeclareBlock, Turn.Human, 6u);
		PlayDialog(3.5f, "NPE/Game04/Turn07/ViperNang_12", OpponentPortrait, WwiseEvents.vo_assassin_npe_110.EventName, Turn.Human, 6u, Phase.Combat, Step.DeclareBlock);
		PlayDialog(1.5f, "NPE/Game04/Turn07/Sparky_07", SparkyPortrait, WwiseEvents.vo_sparky_sep_111.EventName, Turn.Human, 6u, Phase.Combat, Step.DeclareBlock);
		DoPause(0.5f, Turn.Human, 6u, Phase.Main2);
		PlayDialog(3.5f, "NPE/Game04/Turn07/ViperNang_13", OpponentPortrait, WwiseEvents.vo_assassin_npe_112.EventName, Turn.Human, 6u, Phase.Main2);
		PlayDialog(4.5f, "NPE/Game04/Turn08/ViperNang_14", OpponentPortrait, WwiseEvents.vo_assassin_npe_114.EventName, Turn.AI, 7u, Phase.Main2);
		PlayDialog(3.5f, "NPE/Game04/Turn08/ViperNang_15a", OpponentPortrait, WwiseEvents.vo_assassin_npe_115_alt.EventName, Turn.Human, 7u, Phase.Ending, Step.End);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.MissingKeyAttacker, "NPE/Game01/Turn03/Interceptor_14", Phase.Combat, Step.DeclareAttack, Turn.Human, 7u);
		AI_Casts(Turn.Human, 7u, "Nimble Pilferer", Phase.Ending, Step.End);
		AI_Casts(Turn.Human, 7u, "Nimble Pilferer", Phase.Ending, Step.End);
		AI_Casts(Turn.Human, 7u, "Nimble Pilferer", Phase.Ending, Step.End);
		PlayDialog(2.5f, "NPE/Game04/Turn09/ViperNang_16", OpponentPortrait, WwiseEvents.vo_assassin_npe_116.EventName, Turn.AI, 8u, Phase.Combat, Step.BeginCombat);
		PlayTriggerableDialog(2.5f, "NPE/Game04/Turn09/Sparky_09", SparkyPortrait, WwiseEvents.vo_sparky_sep_117.EventName, Turn.AI, 8u, "Nimble Pilferer", TriggerCondition.IsAttacking);
		AddInterceptorDialogForKeyCard("Confront the Assault", InterceptionType.PassingWithSpellNotCast, "NPE/Game04/Turn08/Interceptor_66", Phase.Combat, Step.DeclareAttack, Turn.AI, 8u);
		AI_AttackAll(8u);
		AddInterceptorDialogForKeyCard("Nimble Pilferer", InterceptionType.MissingKeyBlock, "NPE/Game04/Turn06/Interceptor_61", Phase.Combat, Step.DeclareBlock, Turn.AI, 8u);
		PlayDialog(2f, "NPE/Game04/Turn09/ViperNang_17", OpponentPortrait, WwiseEvents.vo_assassin_npe_118.EventName, Turn.AI, 8u, Phase.Main2);
		PlayDialog(3f, "NPE/Game04/Turn09/Sparky_10", SparkyPortrait, WwiseEvents.vo_sparky_sep_119.EventName, Turn.Human, 8u);
		AddInterceptorDialogForKeyCard("Angelic Reward", InterceptionType.PassingWithSpellNotCast, "NPE/Game03/Turn02/Interceptor_47", Phase.Main1, Step.None, Turn.Human, 8u);
		AddInterceptorDialogForKeyCard("Shrine Keeper", InterceptionType.MissingKeyAttacker, "NPE/Game01/Turn03/Interceptor_14", Phase.Combat, Step.DeclareAttack, Turn.Human, 8u);
		PlayDialog(4f, "NPE/Game04/Turn11/Sparky_11", SparkyPortrait, WwiseEvents.vo_sparky_107.EventName, Turn.Human, 11u, Phase.Main2);
		AI_Casts(Turn.AI, 12u, "Soulhunter Rakshasa");
	}
}
