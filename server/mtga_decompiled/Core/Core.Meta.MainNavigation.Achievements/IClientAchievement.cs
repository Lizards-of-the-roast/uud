using System;
using System.Threading.Tasks;
using Core.Meta.MainNavigation.Achievements.Scripts;
using UnityEngine;
using Wizards.Unification.Models.Graph;

namespace Core.Meta.MainNavigation.Achievements;

public interface IClientAchievement
{
	GraphIdNodeId Id { get; }

	IClientAchievementGroup AchievementGroup { get; }

	string TitleLocalizationKey { get; }

	string DescriptionLocalizationKey { get; }

	string ParentheticalTextLocalizationKey { get; }

	string Title { get; }

	string Description { get; }

	int CurrentCount { get; }

	int LastSeenCount { get; }

	int MaxCount { get; }

	bool IsFavorite { get; }

	bool IsCompleted { get; }

	bool IsClaimable { get; }

	bool IsClaimed { get; }

	string ArtId { get; }

	AchievementUpNextReason UpNextReason { get; set; }

	AchievementReward Reward { get; }

	event Action OnRewardClaimed;

	Task<Sprite> GetIconAsync();

	void SetFavorite(bool isFavorite);

	void UpdateStateWithDeltas(ClientNodeState preState, ClientNodeState postState);
}
