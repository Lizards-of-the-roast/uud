using System.Collections.Generic;

namespace Wotc.Mtga.Events;

public static class CampaignGraphMilestonesUtilities
{
	public static readonly IReadOnlyDictionary<CampaignGraphMilestones, CampaignGraphMilestoneInformation> CampaignGraphMilestoneIdMapping = new Dictionary<CampaignGraphMilestones, CampaignGraphMilestoneInformation>
	{
		{
			CampaignGraphMilestones.OpenDualColorPreconEvent,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "OpenDualColorPreconEvent")
		},
		{
			CampaignGraphMilestones.OpenSparkQueue,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "OpenSparkQueue")
		},
		{
			CampaignGraphMilestones.GraduateSparkRank,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "GraduateSparkRank")
		},
		{
			CampaignGraphMilestones.OpenSparkyDeckDuel,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "OpenSparkyDeckDuel")
		},
		{
			CampaignGraphMilestones.CloseSparkyDeckDuel,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "CloseSparkyDeckDuel")
		},
		{
			CampaignGraphMilestones.SCIntroDone,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "SCIntroDone")
		},
		{
			CampaignGraphMilestones.SparkRankTier1,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "SparkRankTier1")
		},
		{
			CampaignGraphMilestones.SparkRankTier2,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "SparkRankTier2")
		},
		{
			CampaignGraphMilestones.SparkRankTier3,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "SparkRankTier3")
		},
		{
			CampaignGraphMilestones.OpenAlchemyPlayQueue,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "OpenAlchemyPlayQueue")
		},
		{
			CampaignGraphMilestones.PlayedThroughSparkRank,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "PlayedThroughSparkRank")
		},
		{
			CampaignGraphMilestones.OpenJumpInEvent,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "OpenJumpInEvent")
		},
		{
			CampaignGraphMilestones.NPE_Completed,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "NPE_Completed")
		},
		{
			CampaignGraphMilestones.SkippedNPEV2FromNPEV1Migration,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "SkippedNPEV2FromNPEV1Migration")
		},
		{
			CampaignGraphMilestones.OpenQuickDraftAndSealedEvents,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "OpenQuickDraftAndSealedEvents")
		},
		{
			CampaignGraphMilestones.Unlocked_StarterPrecon_2023_WU,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Unlocked_StarterPrecon_2023_WU")
		},
		{
			CampaignGraphMilestones.Unlocked_StarterPrecon_2023_UB,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Unlocked_StarterPrecon_2023_UB")
		},
		{
			CampaignGraphMilestones.Unlocked_StarterPrecon_2023_BR,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Unlocked_StarterPrecon_2023_BR")
		},
		{
			CampaignGraphMilestones.Unlocked_StarterPrecon_2023_RG,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Unlocked_StarterPrecon_2023_RG")
		},
		{
			CampaignGraphMilestones.Unlocked_StarterPrecon_2023_GW,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Unlocked_StarterPrecon_2023_GW")
		},
		{
			CampaignGraphMilestones.Unlocked_StarterPrecon_2023_WB,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Unlocked_StarterPrecon_2023_WB")
		},
		{
			CampaignGraphMilestones.Unlocked_StarterPrecon_2023_UR,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Unlocked_StarterPrecon_2023_UR")
		},
		{
			CampaignGraphMilestones.Unlocked_StarterPrecon_2023_BG,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Unlocked_StarterPrecon_2023_BG")
		},
		{
			CampaignGraphMilestones.Unlocked_StarterPrecon_2023_RW,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Unlocked_StarterPrecon_2023_RW")
		},
		{
			CampaignGraphMilestones.Unlocked_StarterPrecon_2023_GU,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Unlocked_StarterPrecon_2023_GU")
		},
		{
			CampaignGraphMilestones.OpenHistoricBrawlQueue,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "OpenHistoricBrawlQueue")
		},
		{
			CampaignGraphMilestones.OpenAlchemyRankQueue,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "OpenAlchemyRankQueue")
		},
		{
			CampaignGraphMilestones.AlchemyBronzeTier1,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "AlchemyBronzeTier1")
		},
		{
			CampaignGraphMilestones.AlchemyBronzeTier2,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "AlchemyBronzeTier2")
		},
		{
			CampaignGraphMilestones.AlchemyBronzeTier3,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "AlchemyBronzeTier3")
		},
		{
			CampaignGraphMilestones.GraduateAlchemyBronzeRank,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "GraduateAlchemyBronzeRank")
		},
		{
			CampaignGraphMilestones.Play5BrawlGames,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "Play5BrawlGames")
		},
		{
			CampaignGraphMilestones.SparkRank1GamePlayed,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "SparkRank1GamePlayed")
		},
		{
			CampaignGraphMilestones.SparkRank2GamesPlayed,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "SparkRank2GamesPlayed")
		},
		{
			CampaignGraphMilestones.GrantHistoricBrawlStarterDeck1,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "GrantHistoricBrawlStarterDeck1")
		},
		{
			CampaignGraphMilestones.GrantHistoricBrawlStarterDeck2,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "GrantHistoricBrawlStarterDeck2")
		},
		{
			CampaignGraphMilestones.NPEv4_Landing,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "NPEv4_Landing")
		},
		{
			CampaignGraphMilestones.ColorChallengeComplete,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "ColorChallengeComplete")
		},
		{
			CampaignGraphMilestones.CompletedDualColorPreconEvent,
			new CampaignGraphMilestoneInformation("NewPlayerExperience", "CompletedDualColorPreconEvent")
		}
	};

	public static bool? MilestoneCompleted(this CampaignGraphMilestones milestone, CampaignGraphManager graphManager = null)
	{
		return CampaignGraphMilestoneIdMapping[milestone].MilestoneCompleted(graphManager);
	}

	public static bool? IsMilestoneCompleted(CampaignGraphMilestones milestone, CampaignGraphManager graphManager = null)
	{
		return milestone.MilestoneCompleted(graphManager);
	}

	public static bool? CheckIfUserIsBetweenMilestones(CampaignGraphMilestones milestoneOne, CampaignGraphMilestones milestoneTwo)
	{
		bool? flag = milestoneOne.MilestoneCompleted();
		bool? flag2 = milestoneTwo.MilestoneCompleted();
		if (!flag.HasValue || !flag2.HasValue)
		{
			return null;
		}
		return flag.Value ^ flag2.Value;
	}
}
