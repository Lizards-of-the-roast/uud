using System;
using System.Threading.Tasks;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Achievements.Scripts;
using UnityEngine;
using Wizards.Arena.Enums.CampaignGraph;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Unification.Models.Graph;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Achievements;

public class Achievement : IClientAchievement
{
	private readonly IAchievementManager _achievementManager;

	private readonly IClientLocProvider _localizationProvider;

	private readonly CampaignGraphManager _campaignGraphManager;

	private GraphIdNodeId _id;

	private readonly IClientAchievementGroup _achievementGroup;

	private int? _lastSeenCount;

	private readonly AchievementReward _reward;

	public GraphIdNodeId Id => _id;

	public IClientAchievementGroup AchievementGroup => _achievementGroup;

	private ClientNodeDefinition NodeDefinition
	{
		get
		{
			if (!_campaignGraphManager.TryGetNodeDefinition(_id, out var nodeDefinition))
			{
				SimpleLog.LogErrorFormat("Could not find the node definition for " + _id.GraphId + "." + _id.NodeId + ".");
			}
			return nodeDefinition;
		}
	}

	private ClientNodeState NodeState
	{
		get
		{
			_campaignGraphManager.TryGetNodeState(_id, out var nodeState);
			return nodeState;
		}
	}

	public string TitleLocalizationKey
	{
		get
		{
			ClientNodeDefinition nodeDefinition = NodeDefinition;
			if (nodeDefinition != null)
			{
				return nodeDefinition.UXInfo.AchievementUXInfo.TitleLocKey;
			}
			return string.Empty;
		}
	}

	public string DescriptionLocalizationKey
	{
		get
		{
			ClientNodeDefinition nodeDefinition = NodeDefinition;
			if (nodeDefinition != null)
			{
				return nodeDefinition.UXInfo.AchievementUXInfo.DescriptionLocKey;
			}
			return string.Empty;
		}
	}

	public string ParentheticalTextLocalizationKey => NodeDefinition?.UXInfo?.AchievementUXInfo?.HoverTextLocKey ?? string.Empty;

	public string Title => _localizationProvider.GetLocalizedText(TitleLocalizationKey);

	public string Description => _localizationProvider.GetLocalizedText(DescriptionLocalizationKey);

	public string HoverText => _localizationProvider.GetLocalizedText(ParentheticalTextLocalizationKey);

	public int CurrentCount => NodeDefinition?.GetProgress(NodeState) ?? (-1);

	public int LastSeenCount => _lastSeenCount.GetValueOrDefault();

	public int MaxCount => NodeDefinition?.GetMaxProgress() ?? (-1);

	public bool IsFavorite => _achievementManager.IsAchievementFavorited(this);

	public bool IsCompleted
	{
		get
		{
			ClientNodeState nodeState = NodeState;
			if (nodeState == null)
			{
				SimpleLog.LogError("No graph state has been defined for this achievement " + _id.GraphId + "." + _id.NodeId + ". This is needed to determine completion status.");
				return false;
			}
			return nodeState.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed;
		}
	}

	public bool IsClaimable
	{
		get
		{
			if (IsCompleted)
			{
				return !IsClaimed;
			}
			return false;
		}
	}

	public bool IsClaimed
	{
		get
		{
			if (!_campaignGraphManager.TryGetNodeState(_reward.RewardNodeId, out var nodeState))
			{
				return false;
			}
			return nodeState.Status == Wizards.Arena.Enums.CampaignGraph.NodeStateStatus.Completed;
		}
	}

	public string ArtId => NodeDefinition?.UXInfo.AchievementUXInfo.IconArtId ?? string.Empty;

	public AchievementUpNextReason UpNextReason { get; set; }

	public AchievementReward Reward => _reward;

	public event Action OnRewardClaimed;

	public async Task<Sprite> GetIconAsync()
	{
		return null;
	}

	public void UpdateStateWithDeltas(ClientNodeState preState, ClientNodeState postState)
	{
		ClientNodeDefinition nodeDefinition = NodeDefinition;
		int valueOrDefault = _lastSeenCount.GetValueOrDefault();
		if (!_lastSeenCount.HasValue)
		{
			valueOrDefault = nodeDefinition?.GetProgress(preState) ?? (-1);
			_lastSeenCount = valueOrDefault;
		}
	}

	private Achievement()
	{
		_achievementManager = Pantry.Get<IAchievementManager>();
		_localizationProvider = Pantry.Get<IClientLocProvider>();
		_campaignGraphManager = Pantry.Get<CampaignGraphManager>();
	}

	public Achievement(GraphIdNodeId achievementId, IClientAchievementGroup parentGroup, ClientNodeState previousNodeState = null)
		: this()
	{
		_id = achievementId;
		_achievementGroup = parentGroup;
		ClientNodeDefinition nodeDefinition = NodeDefinition;
		ClientNodeState nodeState = NodeState;
		_lastSeenCount = nodeDefinition.GetProgress(nodeState);
		if (previousNodeState != null)
		{
			_lastSeenCount = nodeDefinition.GetProgress(previousNodeState);
		}
		GraphIdNodeId graphIdNodeId = GraphIdNodeId.From(achievementId.GraphId, nodeDefinition.UXInfo.AchievementUXInfo.RewardNodeId);
		if (_campaignGraphManager.TryGetNodeDefinition(graphIdNodeId, out var nodeDefinition2) && nodeDefinition2?.Configuration != null)
		{
			_reward = new AchievementReward(graphIdNodeId);
		}
	}

	public void SetFavorite(bool isFavorite)
	{
		bool isFavorite2 = IsFavorite;
		if (isFavorite != isFavorite2)
		{
			if (!isFavorite2)
			{
				((AchievementManager)_achievementManager).SetFavorite(_id);
			}
			else
			{
				((AchievementManager)_achievementManager).RemoveFavorite(_id);
			}
		}
	}

	public Promise<ClientCampaignGraphState> ClaimAchievement()
	{
		return _campaignGraphManager.ProcessNode(_reward.RewardNodeId.GraphId, _reward.RewardNodeId.NodeId).ThenOnMainThread(delegate(Promise<ClientCampaignGraphState> p)
		{
			if (p.Successful)
			{
				this.OnRewardClaimed?.Invoke();
				_achievementManager.RemoveClaimedAchievementFromClaimables(this);
			}
		});
	}

	public static IClientAchievement FromIClientAchievement(IClientAchievement achievementSource)
	{
		return new Achievement
		{
			_id = achievementSource.Id,
			_lastSeenCount = achievementSource.LastSeenCount
		};
	}
}
