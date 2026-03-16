using System;
using System.Collections.Generic;
using Wizards.Unification.Models.Graph;

namespace Core.Meta.MainNavigation.Achievements;

public interface IAchievementManager
{
	IReadOnlyCollection<IClientAchievement> FavoriteAchievements { get; }

	bool CachePopulated { get; }

	IReadOnlyList<IClientAchievementGroup> ClaimableAchievementGroups { get; }

	int ClaimableAchievementCount { get; }

	IEnumerable<IClientAchievementSet> AchievementSets { get; }

	IEnumerable<IClientAchievement> Achievements { get; }

	IEnumerable<IClientAchievementGroup> AchievementGroups { get; }

	IEnumerable<IClientAchievement> UpNextAchievements { get; }

	event Action<IClientAchievement> OnHomePageAchievementsUpdated;

	event Action<List<IClientAchievement>> OnFavoriteAchievementsUpdated;

	IEnumerable<IClientAchievement> GetQuestTrackerAchievements(int count);

	void RemoveClaimedAchievementFromClaimables(IClientAchievement clientAchievement);

	bool IsAchievementFavorited(IClientAchievement achievement);

	bool IsAchievementFavorited(GraphIdNodeId id);
}
