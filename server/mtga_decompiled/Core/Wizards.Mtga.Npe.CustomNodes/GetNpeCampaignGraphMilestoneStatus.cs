using System.Collections.Generic;
using Unity.VisualScripting;
using Wotc.Mtga.Events;

namespace Wizards.Mtga.Npe.CustomNodes;

[UnitCategory("NPE")]
public class GetNpeCampaignGraphMilestoneStatus : Unit
{
	private static readonly Dictionary<Milestones, string> _milestoneMapping = new Dictionary<Milestones, string>
	{
		{
			Milestones.Unknown,
			null
		},
		{
			Milestones.OpenDualColorPreconEvent,
			"OpenDualColorPreconEvent"
		},
		{
			Milestones.OpenSparkQueue,
			"OpenSparkQueue"
		},
		{
			Milestones.GraduateSparkRank,
			"GraduateSparkRank"
		},
		{
			Milestones.OpenSparkyDeckDuel,
			"OpenSparkyDeckDuel"
		},
		{
			Milestones.CloseSparkyDeckDuel,
			"CloseSparkyDeckDuel"
		},
		{
			Milestones.SCIntroDone,
			"SCIntroDone"
		},
		{
			Milestones.SparkRankTier1,
			"SparkRankTier1"
		},
		{
			Milestones.SparkRankTier2,
			"SparkRankTier2"
		},
		{
			Milestones.SparkRankTier3,
			"SparkRankTier3"
		},
		{
			Milestones.OpenAlchemyPlayQueue,
			"OpenAlchemyPlayQueue"
		},
		{
			Milestones.PlayedThroughSparkRank,
			"PlayedThroughSparkRank"
		},
		{
			Milestones.OpenJumpInEvent,
			"OpenJumpInEvent"
		},
		{
			Milestones.NPE_Completed,
			"NPE_Completed"
		},
		{
			Milestones.SkippedNPEV2FromNPEV1Migration,
			"SkippedNPEV2FromNPEV1Migration"
		},
		{
			Milestones.OpenQuickDraftAndSealedEvents,
			"OpenQuickDraftAndSealedEvents"
		},
		{
			Milestones.Unlocked_StarterPrecon_2023_WU,
			"Unlocked_StarterPrecon_2023_WU"
		},
		{
			Milestones.Unlocked_StarterPrecon_2023_UB,
			"Unlocked_StarterPrecon_2023_UB"
		},
		{
			Milestones.Unlocked_StarterPrecon_2023_BR,
			"Unlocked_StarterPrecon_2023_BR"
		},
		{
			Milestones.Unlocked_StarterPrecon_2023_RG,
			"Unlocked_StarterPrecon_2023_RG"
		},
		{
			Milestones.Unlocked_StarterPrecon_2023_GW,
			"Unlocked_StarterPrecon_2023_GW"
		},
		{
			Milestones.Unlocked_StarterPrecon_2023_WB,
			"Unlocked_StarterPrecon_2023_WB"
		},
		{
			Milestones.Unlocked_StarterPrecon_2023_UR,
			"Unlocked_StarterPrecon_2023_UR"
		},
		{
			Milestones.Unlocked_StarterPrecon_2023_BG,
			"Unlocked_StarterPrecon_2023_BG"
		},
		{
			Milestones.Unlocked_StarterPrecon_2023_RW,
			"Unlocked_StarterPrecon_2023_RW"
		},
		{
			Milestones.Unlocked_StarterPrecon_2023_GU,
			"Unlocked_StarterPrecon_2023_GU"
		},
		{
			Milestones.OpenHistoricBrawlQueue,
			"OpenHistoricBrawlQueue"
		},
		{
			Milestones.OpenAlchemyRankQueue,
			"OpenAlchemyRankQueue"
		},
		{
			Milestones.AlchemyBronzeTier1,
			"AlchemyBronzeTier1"
		},
		{
			Milestones.AlchemyBronzeTier2,
			"AlchemyBronzeTier2"
		},
		{
			Milestones.AlchemyBronzeTier3,
			"AlchemyBronzeTier3"
		},
		{
			Milestones.GraduateAlchemyBronzeRank,
			"GraduateAlchemyBronzeRank"
		},
		{
			Milestones.Play5BrawlGames,
			"Play5BrawlGames"
		},
		{
			Milestones.SparkRank1GamePlayed,
			"SparkRank1GamePlayed"
		},
		{
			Milestones.SparkRank2GamesPlayed,
			"SparkRank2GamesPlayed"
		},
		{
			Milestones.GrantHistoricBrawlStarterDeck1,
			"GrantHistoricBrawlStarterDeck1"
		},
		{
			Milestones.GrantHistoricBrawlStarterDeck2,
			"GrantHistoricBrawlStarterDeck2"
		},
		{
			Milestones.NPEv4_Landing,
			"NPEv4_Landing"
		}
	};

	[DoNotSerialize]
	[PortLabel("Milestone")]
	private ValueInput _milestone;

	[DoNotSerialize]
	[PortLabel("Override Status")]
	private ValueInput _overrideStatus;

	[DoNotSerialize]
	[PortLabel("Milestone Status")]
	private ValueOutput _milestoneStatus;

	private bool _milestoneCompleted;

	[PortLabelHidden]
	[DoNotSerialize]
	public ControlInput enter { get; private set; }

	[PortLabelHidden]
	[DoNotSerialize]
	public ControlOutput exit { get; private set; }

	protected override void Definition()
	{
		enter = ControlInput("enter", Enter);
		exit = ControlOutput("exit");
		_milestone = ValueInput("_milestone", Milestones.Unknown);
		_overrideStatus = ValueInput("_overrideStatus", UnitStatusOverride.NoOverride);
		_milestoneStatus = ValueOutput("_milestoneStatus", (Flow x) => _milestoneCompleted);
		Succession(enter, exit);
		Requirement(_milestone, _milestoneStatus);
	}

	private ControlOutput Enter(Flow flow)
	{
		Pantry.Get<CampaignGraphManager>().TryGetState("NewPlayerExperience", out var state);
		state.MilestoneStates.TryGetValue(_milestoneMapping[flow.GetValue<Milestones>(_milestone)], out var value);
		_milestoneCompleted = value;
		return exit;
	}
}
