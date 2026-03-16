namespace Wotc.Mtga.Events;

public static class IEventInfoExtensions
{
	public static bool ShouldBadgeOnEventItem(this IEventInfo eventInfo)
	{
		return ShouldBadgeOnEventItem(eventInfo.EventId);
	}

	public static bool ShouldBadgeOnEventItem(string eventId)
	{
		switch (eventId)
		{
		case "ColorChallenge":
		{
			bool? flag5 = CampaignGraphMilestones.ColorChallengeComplete.MilestoneCompleted();
			if (flag5.HasValue)
			{
				return !flag5.Value;
			}
			return false;
		}
		case "DualColorPrecons":
		{
			bool? flag3 = CampaignGraphMilestones.CompletedDualColorPreconEvent.MilestoneCompleted();
			if (flag3.HasValue)
			{
				return !flag3.Value;
			}
			return false;
		}
		case "Spark_Alchemy_Ladder":
		case "Spark_Rank_Ladder":
		{
			bool? flag6 = CampaignGraphMilestones.PlayedThroughSparkRank.MilestoneCompleted();
			if (flag6.HasValue)
			{
				return !flag6.Value;
			}
			return false;
		}
		case "SparkyStarterDeckDuel":
		{
			bool? flag2 = CampaignGraphMilestones.CloseSparkyDeckDuel.MilestoneCompleted();
			if (flag2.HasValue)
			{
				return !flag2.Value;
			}
			return false;
		}
		case "StandardRanked":
		{
			bool? flag4 = CampaignGraphMilestones.GraduateAlchemyBronzeRank.MilestoneCompleted();
			if (flag4.HasValue)
			{
				return !flag4.Value;
			}
			return false;
		}
		case "Play_Brawl_Historic":
		{
			bool? flag = CampaignGraphMilestones.Play5BrawlGames.MilestoneCompleted();
			if (flag.HasValue)
			{
				return !flag.Value;
			}
			return false;
		}
		default:
			return false;
		}
	}
}
