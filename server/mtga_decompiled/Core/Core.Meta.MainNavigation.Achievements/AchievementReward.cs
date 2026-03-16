using UnityEngine;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Unification.Models.Graph;
using Wizards.Unification.Models.Player;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementReward : Reward
{
	private readonly GraphIdNodeId _id;

	private ClientNodeDefinition NodeDefinition
	{
		get
		{
			if (!_campaignGraphManager.TryGetNodeDefinition(_id, out var nodeDefinition))
			{
				Debug.LogErrorFormat("Could not find the reward node definition for {0}(Graph ID) {1}(Node ID).", _id.GraphId, _id.NodeId);
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
				Debug.LogErrorFormat("Could not find the node state for {0}(Graph ID) {1}(Node ID).", _id.GraphId, _id.NodeId);
			}
			return nodeState;
		}
	}

	public ClientChestDescription RewardChestDescription => NodeDefinition?.Configuration?.PayoutNodeConfig?.ChestDescription;

	public override string TitleLocKey => RewardChestDescription.headerLocKey;

	public override string DescriptionLocKey => RewardChestDescription.descriptionLocKey;

	public override string Title => _localizationProvider.GetLocalizedText(TitleLocKey);

	public override string Description => _localizationProvider.GetLocalizedText(DescriptionLocKey);

	public override int Amount
	{
		get
		{
			if (!int.TryParse(RewardChestDescription.quantity, out var result))
			{
				return 0;
			}
			return result;
		}
	}

	public override string RewardIconPrefab => RewardChestDescription.prefab;

	public override string CosmeticRewardReferenceID => RewardChestDescription.referenceId;

	public bool IsClaimed => NodeState.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed;

	public GraphIdNodeId RewardNodeId => _id;

	public AchievementReward(GraphIdNodeId rewardNodeId)
	{
		_id = rewardNodeId;
	}
}
