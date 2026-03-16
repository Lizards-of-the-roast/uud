using System.Collections.Generic;

namespace Core.Meta.MainNavigation.Achievements;

public interface IAchievementGroupGoal
{
	int CurrentProgress { get; }

	int MaxCount { get; }

	int GroupRewardsCount { get; }

	IReadOnlyCollection<IAchievementGroupReward> GroupRewards { get; }

	bool TryGetAchievementGroupReward(int threshold, out IAchievementGroupReward achievementGroupReward);
}
