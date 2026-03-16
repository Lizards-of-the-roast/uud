using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;

namespace Core.Meta.MainNavigation.Achievements;

public static class AchievementsScreenHelperFunctions
{
	public const string AchievementScreenPrefabAltKeyName = "AchievementScreen";

	public const string AchievementHubPrefabAltKeyName = "AchievementHub";

	public const string AchievementGroupPrefabAltKeyName = "AchievementGroup";

	public const string AchievementScreenCardAltKeyName = "AchievementScreenCard";

	public const string AchievementPoptartPrefabAltKeyName = "AchievementPoptart";

	public static string GetPrefabPath(AssetLookupSystem assetLookupSystem, string achievementScreenPrefabNameKey)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.PrefabName = achievementScreenPrefabNameKey;
		return assetLookupSystem.TreeLoader.LoadTree<AchievementsScreenPrefab>().GetPayload(assetLookupSystem.Blackboard).PrefabPath;
	}
}
