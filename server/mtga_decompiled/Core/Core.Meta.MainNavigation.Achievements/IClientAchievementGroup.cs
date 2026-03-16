using System;
using System.Collections.Generic;
using Wizards.Unification.Models.Graph;

namespace Core.Meta.MainNavigation.Achievements;

public interface IClientAchievementGroup
{
	GraphIdNodeId GroupId { get; }

	IClientAchievementSet AchievementSet { get; }

	bool HasGroupRewards { get; }

	string TitleLocalizationKey { get; }

	string Title { get; }

	string DescriptionLocalizationKey { get; }

	string Description { get; }

	IAchievementGroupGoal AchievementGroupGoal { get; }

	int CompletedAchievementCount { get; }

	int ClaimableAchievementCount { get; }

	int ClaimedAchievementCount { get; }

	IReadOnlyCollection<IClientAchievement> Achievements { get; }

	float AchievementGroupCompletion { get; }

	bool AchievementGroupCompleted { get; }

	bool AchievementGroupClaimed
	{
		get
		{
			if (ClaimedAchievementCount == TotalAchievementCount)
			{
				return TotalAchievementCount != 0;
			}
			return false;
		}
	}

	int CurrentAchievementCompletionCount { get; }

	int TotalAchievementCompletionCount { get; }

	int TotalAchievementCount { get; }

	event Action GroupChanged;

	bool TryGetAchievementGroupReward(int threshold, out (int threshold, IAchievementGroupReward reward) groupReward);
}
