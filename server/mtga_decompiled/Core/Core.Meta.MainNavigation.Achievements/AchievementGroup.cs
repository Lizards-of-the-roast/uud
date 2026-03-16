using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Unification.Models.Graph;
using Wizards.Unification.Models.Player;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementGroup : IClientAchievementGroup
{
	private readonly IClientLocProvider _localizationProvider;

	private readonly CampaignGraphManager _campaignGraphManager;

	private readonly GraphIdNodeId _id;

	private readonly IClientAchievementSet _achievementSet;

	private readonly List<IClientAchievement> _achievements;

	private readonly IAchievementGroupGoal _achievementGroupGoal;

	private ClientAchievementGroupConfiguration AchievementGroupConfiguration
	{
		get
		{
			if (!_campaignGraphManager.TryGetGraphDefinition(_id.GraphId, out var graphDefinition))
			{
				Debug.LogErrorFormat("Could not find the graph definition for the graph ID: {0}", _id.GraphId);
				return null;
			}
			return graphDefinition?.Configuration?.AchievementConfig?.Groups?.FirstOrDefault((ClientAchievementGroupConfiguration x) => x.Id == _id.NodeId);
		}
	}

	public GraphIdNodeId GroupId => _id;

	public IClientAchievementSet AchievementSet => _achievementSet;

	public bool HasGroupRewards
	{
		get
		{
			IAchievementGroupGoal achievementGroupGoal = _achievementGroupGoal;
			if (achievementGroupGoal == null)
			{
				return false;
			}
			return achievementGroupGoal.GroupRewardsCount > 0;
		}
	}

	public string TitleLocalizationKey => AchievementGroupConfiguration.TitleLocKey;

	public string Title => _localizationProvider.GetLocalizedText(AchievementGroupConfiguration.TitleLocKey);

	public string DescriptionLocalizationKey => AchievementGroupConfiguration.DescriptionLocKey;

	public string Description => _localizationProvider.GetLocalizedText(AchievementGroupConfiguration.DescriptionLocKey);

	public IReadOnlyCollection<IClientAchievement> Achievements => _achievements;

	public float AchievementGroupCompletion => (float)_achievementGroupGoal.CurrentProgress / (float)_achievementGroupGoal.MaxCount;

	public bool AchievementGroupCompleted => _achievementGroupGoal.CurrentProgress == _achievementGroupGoal.MaxCount;

	public int CurrentAchievementCompletionCount => _achievementGroupGoal.CurrentProgress;

	public int TotalAchievementCompletionCount => _achievements.Select((IClientAchievement x) => x.MaxCount).Sum();

	public IAchievementGroupGoal AchievementGroupGoal => _achievementGroupGoal;

	public int CompletedAchievementCount => _achievements.Count((IClientAchievement x) => x.IsCompleted);

	public int ClaimableAchievementCount => _achievements.Count((IClientAchievement x) => x.IsClaimable);

	public int ClaimedAchievementCount => _achievementGroupGoal.CurrentProgress;

	public int TotalAchievementCount => _achievements.Count;

	public event Action GroupChanged;

	private AchievementGroup()
	{
		_localizationProvider = Pantry.Get<IClientLocProvider>();
		_campaignGraphManager = Pantry.Get<CampaignGraphManager>();
	}

	public AchievementGroup(GraphIdNodeId id, IClientAchievementSet achievementSet)
		: this()
	{
		_id = id;
		_achievementSet = achievementSet;
		ClientAchievementGroupConfiguration achievementGroupConfiguration = AchievementGroupConfiguration;
		List<IAchievementGroupReward> list = new List<IAchievementGroupReward>();
		if (_campaignGraphManager.TryGetGraphDefinition(_id.GraphId, out var graphDefinition))
		{
			ClientNodeDefinition clientNodeDefinition = graphDefinition.Nodes[AchievementGroupConfiguration.MetaGoalNode];
			foreach (var (rewardThreshold, _) in clientNodeDefinition.Configuration?.AccumulativePayoutConfig?.ChestDescriptions ?? new Dictionary<int, ClientChestDescription>())
			{
				list.Add(new AchievementGroupReward(GraphIdNodeId.From(_id.GraphId, clientNodeDefinition.Id), rewardThreshold));
			}
		}
		_achievementGroupGoal = new AchievementGroupGoal(GraphIdNodeId.From(_id.GraphId, achievementGroupConfiguration.MetaGoalNode), list);
		_achievements = new List<IClientAchievement>(achievementGroupConfiguration.AchievementNodes.Count);
		_ = _campaignGraphManager.GetDefinitions().Result;
		foreach (string achievementNode in achievementGroupConfiguration.AchievementNodes)
		{
			try
			{
				IClientAchievement clientAchievement = new Achievement(GraphIdNodeId.From(_id.GraphId, achievementNode), this);
				_achievements.Add(clientAchievement);
				clientAchievement.OnRewardClaimed += AchievementRewardClaimed;
			}
			catch (Exception ex)
			{
				Debug.LogErrorFormat("Couldn't serialize the achievement Node ID for {0} in the {1} set.\nException: {2}", achievementNode, _id.GraphId, ex);
			}
		}
	}

	~AchievementGroup()
	{
		foreach (IClientAchievement achievement in _achievements)
		{
			achievement.OnRewardClaimed -= AchievementRewardClaimed;
		}
	}

	private void AchievementRewardClaimed()
	{
		this.GroupChanged?.Invoke();
	}

	public bool TryGetAchievementGroupReward(int threshold, out (int threshold, IAchievementGroupReward reward) groupReward)
	{
		if (_achievementGroupGoal.TryGetAchievementGroupReward(threshold, out var achievementGroupReward))
		{
			groupReward = (threshold: threshold, reward: achievementGroupReward);
			return true;
		}
		groupReward = (threshold: -1, reward: null);
		return false;
	}
}
