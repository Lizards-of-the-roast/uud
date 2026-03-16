using System;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Achievements;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Wrapper;

namespace Core.Achievements;

internal class AchievementsProgressProvider : IAchievementsProgressProvider
{
	private readonly Dictionary<CollationMapping, string> _collationMappingToGraphIdMap = new Dictionary<CollationMapping, string>();

	private readonly Dictionary<string, IClientAchievementSet> _currentlyCachedAchievementSets = new Dictionary<string, IClientAchievementSet>();

	private readonly Dictionary<string, IClientAchievementGroup> _currentlyCachedAchievementGroups = new Dictionary<string, IClientAchievementGroup>();

	private readonly Dictionary<GraphIdNodeId, IClientAchievement> _currentlyCachedAchievements = new Dictionary<GraphIdNodeId, IClientAchievement>();

	public IReadOnlyDictionary<GraphIdNodeId, IClientAchievement> Achievements => _currentlyCachedAchievements;

	public IReadOnlyDictionary<string, IClientAchievementSet> AchievementSets => _currentlyCachedAchievementSets;

	public IReadOnlyDictionary<string, IClientAchievementGroup> AchievementGroups => _currentlyCachedAchievementGroups;

	public bool CachePopulated { get; }

	private AchievementsProgressProvider()
	{
	}

	internal AchievementsProgressProvider(IReadOnlyDictionary<string, ClientGraphDefinition> clientAchievementSetGraphDefinitions, List<string> achievementSetGraphIds)
		: this()
	{
		foreach (string achievementSetGraphId in achievementSetGraphIds)
		{
			if (!clientAchievementSetGraphDefinitions.TryGetValue(achievementSetGraphId, out var value))
			{
				continue;
			}
			CollationMapping key = CollationMapping.None;
			_collationMappingToGraphIdMap[key] = achievementSetGraphId;
			AchievementSet achievementSet = null;
			try
			{
				achievementSet = (AchievementSet)(_currentlyCachedAchievementSets[achievementSetGraphId] = new AchievementSet(value));
			}
			catch (Exception ex)
			{
				SimpleLog.LogErrorFormat("There was an error creating the client data models for the set: {0} (See Attached Error Below.\n{1}", achievementSetGraphId, ex);
			}
			IEnumerable<IClientAchievementGroup> enumerable = achievementSet?.AchievementGroups;
			foreach (IClientAchievementGroup item in enumerable ?? Enumerable.Empty<IClientAchievementGroup>())
			{
				try
				{
					_currentlyCachedAchievementGroups[item.GroupId.NodeId] = item;
					foreach (IClientAchievement achievement in item.Achievements)
					{
						_currentlyCachedAchievements[achievement.Id] = achievement;
					}
				}
				catch (Exception ex2)
				{
					SimpleLog.LogErrorFormat("There was an error parsing the definition of the achievement group {0}. See attached error.\n{1}", item.GroupId, ex2);
				}
			}
		}
		CachePopulated = true;
	}

	public IClientAchievementSet GetAchievementSet(CollationMapping set)
	{
		_collationMappingToGraphIdMap.TryGetValue(set, out var value);
		if (!string.IsNullOrEmpty(value))
		{
			return GetAchievementSet(value);
		}
		return null;
	}

	public IClientAchievementSet GetAchievementSet(string setGraphId)
	{
		_currentlyCachedAchievementSets.TryGetValue(setGraphId, out var value);
		if (value != null)
		{
			return value;
		}
		return null;
	}

	public IClientAchievementGroup GetAchievementGroup(string groupId)
	{
		_currentlyCachedAchievementGroups.TryGetValue(groupId, out var value);
		if (value != null)
		{
			return value;
		}
		return null;
	}

	public IClientAchievement GetAchievement(GraphIdNodeId achievementId)
	{
		_currentlyCachedAchievements.TryGetValue(achievementId, out var value);
		if (value != null)
		{
			return value;
		}
		return null;
	}

	public IClientAchievementGroup GetAchievementGroup(GraphIdNodeId groupId)
	{
		_currentlyCachedAchievementGroups.TryGetValue(groupId.NodeId, out var value);
		if (value != null)
		{
			return value;
		}
		return null;
	}
}
