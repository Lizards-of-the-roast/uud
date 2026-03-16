using System.Collections.Generic;
using Core.Meta.MainNavigation.Achievements;
using Wizards.Unification.Models.Graph;

namespace Core.Achievements;

public interface IAchievementsProgressProvider
{
	bool CachePopulated { get; }

	IReadOnlyDictionary<GraphIdNodeId, IClientAchievement> Achievements { get; }

	IReadOnlyDictionary<string, IClientAchievementSet> AchievementSets { get; }

	IReadOnlyDictionary<string, IClientAchievementGroup> AchievementGroups { get; }

	IClientAchievement GetAchievement(GraphIdNodeId achievementId);

	IClientAchievementGroup GetAchievementGroup(GraphIdNodeId groupId);

	IClientAchievementSet GetAchievementSet(string Id);
}
