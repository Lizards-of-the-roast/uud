using System.Collections.Generic;
using Core.Meta.Utilities;
using UnityEngine;
using Wizards.Unification.Models.Graph;
using Wizards.Unification.Models.Player;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementGroupReward : Reward, IAchievementGroupReward
{
	private readonly GraphIdNodeId _groupId;

	private readonly int _rewardGrantThreshold;

	private ClientNodeDefinition MetaRewardsNodeDefinition
	{
		get
		{
			if (!_campaignGraphManager.TryGetNodeDefinition(_groupId, out var nodeDefinition))
			{
				Debug.LogErrorFormat("Could not find the node definition for {0}(Graph ID) {1}(Node ID).", _groupId.GraphId, _groupId.NodeId);
			}
			return nodeDefinition;
		}
	}

	private Dictionary<int, ClientChestDescription> MetaRewardChestDescriptions => MetaRewardsNodeDefinition.Configuration?.AccumulativePayoutConfig?.ChestDescriptions;

	private ClientChestDescription ChestDescription
	{
		get
		{
			Dictionary<int, ClientChestDescription> metaRewardChestDescriptions = MetaRewardChestDescriptions;
			if (metaRewardChestDescriptions == null)
			{
				Debug.LogErrorFormat("There was no chest descriptions found for the given group reward item at: {0}(Graph ID) {1}(Node ID)", _groupId.GraphId, _groupId.NodeId);
				return null;
			}
			if (!metaRewardChestDescriptions.TryGetValue(_rewardGrantThreshold, out var value))
			{
				Debug.LogErrorFormat("There was no chest description found at the threshold: {0}", _rewardGrantThreshold);
				return null;
			}
			return value;
		}
	}

	public string GroupRewardId => $"{_groupId.GraphId}.{_groupId.NodeId}({_rewardGrantThreshold})";

	public override string TitleLocKey => ChestDescription?.headerLocKey ?? string.Empty;

	public override string DescriptionLocKey => ChestDescription?.descriptionLocKey ?? string.Empty;

	public override string Title => _localizationProvider.GetLocalizedText(TitleLocKey);

	public override string Description => _localizationProvider.GetLocalizedText(DescriptionLocKey);

	public override int Amount
	{
		get
		{
			if (!int.TryParse(ChestDescription.quantity, out var result))
			{
				return 1;
			}
			return result;
		}
	}

	public override string RewardIconPrefab => ChestDescription?.prefab ?? string.Empty;

	public string ThumbnailPath
	{
		get
		{
			if (ChestDescription == null)
			{
				return string.Empty;
			}
			if (ChestDescription.image1 != null)
			{
				return ServerRewardUtils.FormatAssetFromServerReference(ChestDescription.image1, ServerRewardFileExtension.PNG);
			}
			if (ChestDescription.image2 != null)
			{
				ServerRewardUtils.FormatAssetFromServerReference(ChestDescription.image2, ServerRewardFileExtension.PNG);
			}
			if (ChestDescription.image3 != null)
			{
				ServerRewardUtils.FormatAssetFromServerReference(ChestDescription.image3, ServerRewardFileExtension.PNG);
			}
			return string.Empty;
		}
	}

	public int RewardGrantThreshold => _rewardGrantThreshold;

	public override string CosmeticRewardReferenceID => ChestDescription?.referenceId ?? string.Empty;

	public AchievementGroupReward(GraphIdNodeId groupId, int rewardThreshold)
	{
		_groupId = groupId;
		_rewardGrantThreshold = rewardThreshold;
	}
}
