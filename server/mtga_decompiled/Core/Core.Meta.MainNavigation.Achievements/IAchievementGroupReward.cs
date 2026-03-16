namespace Core.Meta.MainNavigation.Achievements;

public interface IAchievementGroupReward
{
	string GroupRewardId { get; }

	string TitleLocKey { get; }

	string DescriptionLocKey { get; }

	string Title { get; }

	string Description { get; }

	int Amount { get; }

	string RewardIconPrefab { get; }

	string ThumbnailPath { get; }

	int RewardGrantThreshold { get; }

	string CosmeticRewardReferenceID { get; }
}
