using AssetLookupTree;
using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Prefab;
using Wotc.Mtga.Extensions;

namespace Core.Code.Familiar;

public static class BotControlManager
{
	private static readonly string DEFAULT_BOT_CONFIG = "DefaultBotControlConfiguration";

	public static void SetUpBotTool(BotTool botTool, AssetLookupSystem assetLookupSystem)
	{
		BotControlConfigurationSO botControlConfigurationSO = FetchBotConfig(assetLookupSystem);
		botTool.MinWaitTime = botControlConfigurationSO._minWaitTime;
		botTool.MaxWaitTime = botControlConfigurationSO._maxWaitTime;
		botTool.MaxIdleTime = botControlConfigurationSO._maxIdleTime;
		botTool.SetDeckHeuristic(botControlConfigurationSO._deckHeuristics.SelectRandom());
		botTool.SetAttackConfig(new AttackConfig(botControlConfigurationSO._maxAttackConfigurationsToExplore, botControlConfigurationSO._maxAttackCalculationTime, botControlConfigurationSO._aiCreatureDensityFactor, botControlConfigurationSO._playerCreatureDensityFactor, botControlConfigurationSO._idleAttackTurnsDensityFactor));
		botTool.SetBlockConfig(new BlockConfig(botControlConfigurationSO._maxBlockConfigurationsToExplore, botControlConfigurationSO._maxBlockCalculationTime));
	}

	public static BotControlConfigurationSO FetchBotConfig(AssetLookupSystem assetLookupSystem)
	{
		AssetLookupTree<BotControlConfigScriptableObject> assetLookupTree = assetLookupSystem.TreeLoader.LoadTree<BotControlConfigScriptableObject>();
		IBlackboard blackboard = assetLookupSystem.Blackboard;
		blackboard.Clear();
		blackboard.PrefabName = DEFAULT_BOT_CONFIG;
		return AssetLoader.InstantiateSO<BotControlConfigurationSO>(assetLookupTree.GetPayload(blackboard).PrefabPath);
	}
}
