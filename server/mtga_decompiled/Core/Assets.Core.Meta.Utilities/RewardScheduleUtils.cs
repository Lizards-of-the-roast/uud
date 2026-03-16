using System.Collections.Generic;
using System.Linq;
using Wizards.Mtga.FrontDoorModels;

namespace Assets.Core.Meta.Utilities;

public class RewardScheduleUtils
{
	public static (DailyWeeklyReward, bool) GetCurrentReward(List<DailyWeeklyReward> rewards, int currentWins, int gamesWon)
	{
		DailyWeeklyReward dailyWeeklyReward = rewards.LastOrDefault((DailyWeeklyReward d) => currentWins >= d.wins && currentWins - gamesWon < d.wins) ?? new DailyWeeklyReward();
		bool item = gamesWon > 0 && dailyWeeklyReward.wins > 0;
		return (dailyWeeklyReward, item);
	}

	public static DailyWeeklyReward GetNextReward(List<DailyWeeklyReward> rewards, int currentWins)
	{
		return rewards.OrderBy((DailyWeeklyReward r) => r.wins).FirstOrDefault((DailyWeeklyReward d) => currentWins < d.wins) ?? rewards.LastOrDefault() ?? new DailyWeeklyReward();
	}

	public static DailyWeeklyReward GetLastReward(List<DailyWeeklyReward> rewards)
	{
		DailyWeeklyReward dailyWeeklyReward = rewards.FirstOrDefault() ?? new DailyWeeklyReward();
		foreach (DailyWeeklyReward reward in rewards)
		{
			if (reward.wins > dailyWeeklyReward.wins)
			{
				dailyWeeklyReward = reward;
			}
		}
		return dailyWeeklyReward;
	}
}
