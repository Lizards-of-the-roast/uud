using Wotc.Mtgo.Gre.External.Messaging;

public class G5_NPEGame5_6drop : NPE_Game
{
	protected override void AddGameContent()
	{
		SetBattlefield("AKH");
		SetStartingPlayer(Turn.AI);
		SetOpponentPortrait(NPEController.Actor.Boss);
		AddOnAIDefeatedDialog(2.5f, "NPE/Game05/End/BolasDefeat/NicolBolas_24", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_162.EventName);
		AddOnAIVictoryDialog(3f, "NPE/Game05/End/BolasVictory/NicolBolas_27", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_165.EventName);
		AddOnAIVictoryDialog(3f, "NPE/Game05/End/BolasVictory/NicolBolas_28", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_166.EventName);
		AddInterceptorDialog(InterceptionType.PassingWithLandNotPlayed, "NPE/Game01/Turn01/Interceptor_1", Phase.Main2, Step.None, Turn.Human, 1u, 2u, 3u, 4u, 5u, 6u, 7u);
		AddInterceptorDialog(InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main2, Step.None, Turn.Human, 1u, 2u, 3u, 5u, 6u, 7u, 8u, 9u);
		AddAlwaysOnInterceptorDialog(InterceptionType.BadTargetting, "NPE/Game01/Turn00/AlwaysReminder_4", "NPE/Game01/Turn00/AlwaysReminder_5", "NPE/Game02/Turn00/AlwaysReminder_35");
		AddTopPickForChooseAction("Volcanic Dragon");
		PlayDialog(4f, "NPE/Game05/Intro/NicolBolas_01", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_129.EventName, Turn.AI, 1u);
		PlayDialog(4.5f, "NPE/Game05/Intro/NicolBolas_02", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_130.EventName, Turn.AI, 1u);
		PlayDialog(4.2f, "NPE/Game05/Intro/NicolBolas_03", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_131_v2.EventName, Turn.AI, 1u);
		PlayDialog(3.5f, "NPE/Game05/Turn01/Sparky_02", SparkyPortrait, WwiseEvents.vo_sparky_sep_134.EventName, Turn.Human, 1u);
		AI_Casts(Turn.AI, 2u, "Miasmic Mummy");
		TriggerablePause(2f, Turn.AI, 2u, "Miasmic Mummy", TriggerCondition.AbilityTriggersOrActivates);
		PlayTriggerableDialog(2.8f, "NPE/Game05/Turn02/Sparky_03", SparkyPortrait, WwiseEvents.vo_sparky_sep_135.EventName, Turn.AI, 2u, "Miasmic Mummy", TriggerCondition.EntersBattlefield);
		AddSelectReminder("NPE/Game05/Turn02/PickReminder_67", 1.5f, 1.5f, Turn.AI, 2u, "Plains");
		AddAlwaysOnInterceptorDialog(InterceptionType.ShouldChooseLand, "NPE/Game05/Turn00/AlwaysReminder_68");
		DoPause(1f, Turn.AI, 2u, Phase.Main2);
		PlayDialog(3f, "NPE/Game05/Turn02/Sparky_04", SparkyPortrait, WwiseEvents.vo_sparky_sep_136.EventName, Turn.AI, 2u, Phase.Main2);
		PlayDialog(8f, "NPE/Game05/Turn02/NicolBolas_05", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_137.EventName, Turn.AI, 2u, Phase.Main2);
		AI_Casts(Turn.AI, 3u, "Cruel Cut");
		PlayTriggerableDialog(4.5f, "NPE/Game05/Turn03/NicolBolas_06", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_138.EventName, Turn.AI, 3u, "Shrine Keeper", TriggerCondition.Dies);
		AI_DontAttack(4u);
		SetAIsPreferredTarget("Ambition's Cost", TargetCharacteristics.IsTheAIPlayer);
		AI_Casts(Turn.AI, 4u, "Ambition's Cost", Phase.Main2);
		PlayDialog(4.5f, "NPE/Game05/Turn04/NicolBolas_07", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_139.EventName, Turn.AI, 4u);
		TriggerablePause(3.2f, Turn.AI, 4u, "Ambition's Cost", TriggerCondition.IsCast);
		SetAIsPreferredTarget("Rise from the Grave", "Volcanic Dragon");
		AI_Casts(Turn.AI, 5u, "Rise from the Grave");
		PlayDialog(5.5f, "NPE/Game05/Turn05/NicolBolas_08", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_140.EventName, Turn.AI, 5u);
		TriggerablePause(4.8f, Turn.AI, 5u, "Rise from the Grave", TriggerCondition.IsCast);
		TriggerablePause(1f, Turn.AI, 5u, "Volcanic Dragon", TriggerCondition.EntersBattlefield);
		AI_Casts(Turn.AI, 6u, "Doublecast");
		AI_Casts(Turn.AI, 6u, "Seismic Rupture");
		PlayDialog(2f, "NPE/Game05/Turn06/NicolBolas_09", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_141.EventName, Turn.AI, 6u);
		TriggerablePause(4.5f, Turn.AI, 6u, "Doublecast", TriggerCondition.IsCast);
		TriggerablePause(3.2f, Turn.AI, 6u, "Seismic Rupture", TriggerCondition.IsCast);
		PlayDialog(1f, "NPE/Game05/Turn06/Sparky_05", SparkyPortrait, WwiseEvents.vo_sparky_sep_142.EventName, Turn.AI, 6u, Phase.Combat, Step.BeginCombat);
		SetAIsPreferredTarget("Overflowing Insight", TargetCharacteristics.IsTheAIPlayer);
		AI_Casts(Turn.AI, 7u, "Overflowing Insight", Phase.Main2);
		TriggerablePause(2.8f, Turn.AI, 7u, "Overflowing Insight", TriggerCondition.IsCast);
		PlayTriggerableDialog(3f, "NPE/Game05/Turn07/NicolBolas_10", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_143.EventName, Turn.AI, 7u, "Overflowing Insight", TriggerCondition.SpellFinishesResolving);
		PlayTriggerableDialog(1.5f, "NPE/Game05/Turn07/NicolBolas_12", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_144.EventName, Turn.Human, 7u, "Shrine Keeper", TriggerCondition.EntersBattlefield);
		PlayTriggerableDialog(1.5f, "NPE/Game05/Turn07/NicolBolas_12b", OpponentPortrait, WwiseEvents.vo_nicolbolas_119_huh.EventName, Turn.Human, 7u, "Sanctuary Cat", TriggerCondition.EntersBattlefield);
		PlayTriggerableDialog(3.3f, "NPE/Game05/Turn07/Sparky_06", SparkyPortrait, WwiseEvents.vo_sparky_sep_145.EventName, Turn.Human, 7u, "Take Vengeance", TriggerCondition.IsDrawn);
		AddInterceptorDialogForKeyCard("Take Vengeance", InterceptionType.PassingWithSpellNotCast, "NPE/Game05/Turn07/Interceptor_69", Phase.Main2, Step.None, Turn.Human, 7u);
		PlayTriggerableDialog(4.75f, "NPE/Game05/Turn07/NicolBolas_13", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_146.EventName, Turn.Human, 7u, "Volcanic Dragon", TriggerCondition.Dies);
		AI_Casts(Turn.AI, 8u, "Chaos Maw");
		PlayDialog(1.5f, "NPE/Game05/Turn08/Sparky_07", SparkyPortrait, WwiseEvents.vo_sparky_sep_147.EventName, Turn.AI, 8u, Phase.Main2);
		DoPause(0.5f, Turn.AI, 8u, Phase.Main2);
		PlayDialog(2.7f, "NPE/Game05/Turn08/NicolBolas_14", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_148_v2.EventName, Turn.AI, 8u, Phase.Main2);
		DoPause(1f, Turn.AI, 8u, Phase.Ending, Step.End);
		AddInterceptorDialog(InterceptionType.BadAttack, "NPE/Game01/Turn05/Interceptor_21", Phase.Combat, Step.DeclareAttack, Turn.Human, 8u);
		PlayDialog(3.5f, "NPE/Game05/Turn08/Sparky_08", SparkyPortrait, WwiseEvents.vo_sparky_sep_149.EventName, Turn.Human, 8u, Phase.Combat, Step.DeclareAttack);
		AI_Casts(Turn.AI, 9u, "Volcanic Dragon");
		AI_AttackAll(9u);
		TriggerablePause(2f, Turn.AI, 9u, "Volcanic Dragon", TriggerCondition.IsCast);
		AddBlockingDelayedReminder("NPE/Game05/Turn09/BlockingReminder_70", CheckLocTextDeviceType("NPE/Game01/Turn05/BlockingSubmitReminder_24", "NPE/Game01/Turn05/BlockingSubmitReminder_24_Handheld"), 0.5f, 3f, 9u, "Spirit");
		AddInterceptorDialogForKeyCard("Confront the Assault", InterceptionType.PassingWithSpellNotCast, "NPE/Game05/Turn09/Interceptor_71", Phase.Combat, Step.DeclareAttack, Turn.AI, 9u);
		PlayTriggerableDialog(2f, "NPE/Game05/Turn09/NicolBolas_15", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_150.EventName, Turn.AI, 9u, "Confront the Assault", TriggerCondition.IsCast);
		PlayTriggerableDialog(5.5f, "NPE/Game05/Turn09/NicolBolas_16", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_151_v2.EventName, Turn.AI, 9u, "Plains", TriggerCondition.IsDrawn);
		PlayTriggerableDialog(5f, "NPE/Game05/Turn09/NicolBolas_17", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_152.EventName, Turn.AI, 9u, "Volcanic Dragon", TriggerCondition.Dies);
		AddInterceptorDialog(InterceptionType.PassingWithCreatureNotCast, "NPE/Game01/Turn03/Interceptor_11", Phase.Main2, Step.None, Turn.Human, 9u);
		PlayTriggerableDialog(1.5f, "NPE/Game05/Turn09/NicolBolas_18", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_153.EventName, Turn.Human, 9u, "Sanctuary Cat", TriggerCondition.EntersBattlefield);
		PlayTriggerableDialog(1.8f, "NPE/Game05/Turn09/Sparky_09", SparkyPortrait, WwiseEvents.vo_sparky_sep_154.EventName, Turn.Human, 9u, "Serra Angel", TriggerCondition.EntersBattlefield);
		TriggerableShowHangerOnBattlefield(4f, new HangerSituation
		{
			UseNPEHanger = true,
			HideSummoningSickness = true
		}, Turn.Human, 9u, "Serra Angel", TriggerCondition.EntersBattlefield, showOnLeftSide: true);
		PlayTriggerableDialog(1.5f, "NPE/Game05/Turn10/NicolBolas_19", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_155_v2.EventName, Turn.Human, 9u, "Serra Angel", TriggerCondition.EntersBattlefield);
		PlayTriggerableDialog(4f, "NPE/Game05/Turn10/NicolBolas_20", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_156_v2.EventName, Turn.Human, 9u, "Serra Angel", TriggerCondition.EntersBattlefield);
		PlayTriggerableDialog(3.5f, "NPE/Game05/Turn10/NicolBolas_21", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_157_v2.EventName, Turn.Human, 9u, "Serra Angel", TriggerCondition.EntersBattlefield);
		AI_Casts(Turn.AI, 10u, "Cruel Cut");
		SetAIsPreferredTarget("Cruel Cut", TargetCharacteristics.HumanControls);
		SetAIsPreferredTarget("Cruel Cut", "Inspiring Commander", "Shrine Keeper");
		AI_Casts(Turn.AI, 10u, "Ancient Crab");
		AI_Casts(Turn.AI, 10u, "Renegade Demon");
		AI_AttackAll(10u);
		AddBlockingDelayedReminder("NPE/Game05/Turn010/BlockingReminder_73", CheckLocTextDeviceType("NPE/Game01/Turn05/BlockingSubmitReminder_24", "NPE/Game01/Turn05/BlockingSubmitReminder_24_Handheld"), 0.5f, 3f, 10u, "Sanctuary Cat");
		PlayDialog(4.25f, "NPE/Game05/Turn10/NicolBolas_22", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_158.EventName, Turn.AI, 10u, Phase.Main2);
		PlayDialog(4.5f, "NPE/Game05/Turn10/Sparky_10", SparkyPortrait, WwiseEvents.vo_sparky_sep_159.EventName, Turn.Human, 10u);
		PlayDialog(3f, "NPE/Game05/Turn10/NicolBolas_23", OpponentPortrait, WwiseEvents.vo_nicolbolas_sep_160.EventName, Turn.AI, 11u);
		AI_AttackAll(11u);
		PlayDialog(2f, "NPE/Game05/Turn11/Sparky_11", SparkyPortrait, WwiseEvents.vo_sparky_sep_161.EventName, Turn.Human, 11u);
		AI_AttackAll(12u);
		AI_AttackAll(13u);
		AI_AttackAll(14u);
	}
}
