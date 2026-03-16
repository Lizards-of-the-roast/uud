using System.Collections.Generic;
using Wizards.Mtga;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Events;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementGroupGoal : IAchievementGroupGoal
{
	private readonly Dictionary<int, IAchievementGroupReward> _achievementGroupRewards;

	private readonly GraphIdNodeId _id;

	private readonly CampaignGraphManager _campaignGraphManager;

	private ClientNodeDefinition NodeDefinition
	{
		get
		{
			if (!_campaignGraphManager.TryGetNodeDefinition(_id, out var nodeDefinition))
			{
				SimpleLog.LogErrorFormat("Could not find the node definition for {0}(Graph ID) {1}(Node ID).", _id.GraphId, _id.NodeId);
			}
			return nodeDefinition;
		}
	}

	private ClientNodeState NodeState
	{
		get
		{
			if (!_campaignGraphManager.TryGetNodeState(_id, out var nodeState))
			{
				SimpleLog.LogErrorFormat("Could not find the node state for {0}(Graph ID) {1}(Node ID).", _id.GraphId, _id.NodeId);
			}
			return nodeState;
		}
	}

	public int CurrentProgress
	{
		get
		{
			ClientNodeState nodeState = NodeState;
			if (nodeState != null)
			{
				return NodeDefinition.GetProgress(nodeState);
			}
			return -1;
		}
	}

	public int MaxCount => NodeDefinition.GetMaxProgress();

	public int GroupRewardsCount => _achievementGroupRewards.Count;

	public IReadOnlyCollection<IAchievementGroupReward> GroupRewards => _achievementGroupRewards.Values;

	private AchievementGroupGoal()
	{
		_campaignGraphManager = Pantry.Get<CampaignGraphManager>();
	}

	public AchievementGroupGoal(GraphIdNodeId id, ICollection<IAchievementGroupReward> groupRewards)
		: this()
	{
		_id = id;
		_achievementGroupRewards = new Dictionary<int, IAchievementGroupReward>(groupRewards.Count);
		foreach (IAchievementGroupReward groupReward in groupRewards)
		{
			_achievementGroupRewards[groupReward.RewardGrantThreshold] = groupReward;
		}
	}

	public bool TryGetAchievementGroupReward(int threshold, out IAchievementGroupReward achievementGroupReward)
	{
		return _achievementGroupRewards.TryGetValue(threshold, out achievementGroupReward);
	}
}
