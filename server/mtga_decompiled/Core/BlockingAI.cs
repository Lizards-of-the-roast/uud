using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

public static class BlockingAI
{
	public static BlockConfig Config = DefaultConfig;

	private static Thread _blockAIThread;

	public static BlockConfig DefaultConfig => new BlockConfig(4800u, 6f);

	public static List<BlockingConfiguration> SortedBlockConfigurations { get; private set; } = new List<BlockingConfiguration>();

	public static Stopwatch ThreadTimer { get; private set; } = new Stopwatch();

	public static bool StopExploring { get; private set; } = false;

	public static bool IsCalculating { get; private set; } = false;

	private static void ProduceBlockConfigurations(BlockingConfiguration currentConfiguration, List<BlockingConfiguration> completedConfigurations, uint maxBlockConfigurationsToExplore = uint.MaxValue)
	{
		if (completedConfigurations.Count >= maxBlockConfigurationsToExplore || StopExploring)
		{
			if (completedConfigurations.Count == 0)
			{
				completedConfigurations.Add(currentConfiguration);
			}
			return;
		}
		if (currentConfiguration.AbleBlockerIdToBlockableAttackerIds.Count() == 0)
		{
			completedConfigurations.Add(currentConfiguration);
			if (completedConfigurations.Count < maxBlockConfigurationsToExplore)
			{
				_ = StopExploring;
			}
			return;
		}
		KeyValuePair<uint, HashSet<uint>> keyValuePair = currentConfiguration.AbleBlockerIdToBlockableAttackerIds.First();
		uint key = keyValuePair.Key;
		BlockingConfiguration blockingConfiguration = new BlockingConfiguration(currentConfiguration);
		blockingConfiguration.BlockerAbstains(key);
		ProduceBlockConfigurations(blockingConfiguration, completedConfigurations, maxBlockConfigurationsToExplore);
		if (completedConfigurations.Count > maxBlockConfigurationsToExplore || StopExploring)
		{
			return;
		}
		foreach (uint item in keyValuePair.Value)
		{
			if (completedConfigurations.Count > maxBlockConfigurationsToExplore || StopExploring)
			{
				break;
			}
			BlockingConfiguration blockingConfiguration2 = new BlockingConfiguration(currentConfiguration);
			blockingConfiguration2.CoupleAttackerAndBlocker(item, key);
			ProduceBlockConfigurations(blockingConfiguration2, completedConfigurations, maxBlockConfigurationsToExplore);
			if (completedConfigurations.Count > maxBlockConfigurationsToExplore || StopExploring)
			{
				break;
			}
		}
	}

	public static BlockingConfiguration GetBestBlockConfiguration(SimpleGameStateConstruction gameState, DeclareBlockersRequest request, ICardDatabaseAdapter cardDatabase)
	{
		return GetSortedBlockConfigurations(gameState, request, cardDatabase)[0];
	}

	public static BlockingConfiguration GetBestBlockConfiguration(SimpleGameStateConstruction gameState, BlockingConfiguration initialBlockingConfiguration, ICardDatabaseAdapter cardDatabase)
	{
		List<BlockingConfiguration> sortedBlockConfigurations = GetSortedBlockConfigurations(gameState, initialBlockingConfiguration, cardDatabase);
		if (sortedBlockConfigurations.Count > 0)
		{
			return sortedBlockConfigurations[0];
		}
		return initialBlockingConfiguration;
	}

	public static List<BlockingConfiguration> GetSortedBlockConfigurations(SimpleGameStateConstruction gameState, DeclareBlockersRequest request, ICardDatabaseAdapter cardDatabase)
	{
		BlockingConfiguration initialBlockConfiguration = new BlockingConfiguration(gameState, request);
		return GetSortedBlockConfigurations(gameState, initialBlockConfiguration, cardDatabase);
	}

	public static List<BlockingConfiguration> GetSortedBlockConfigurations(SimpleGameStateConstruction gameState, BlockingConfiguration initialBlockConfiguration, ICardDatabaseAdapter cardDatabase)
	{
		uint num = Config.MaxBlockConfigurationsToExplore;
		if (gameState.StateForReference.ActivePlayer.ClientPlayerEnum == GREPlayerNum.LocalPlayer)
		{
			num /= AttackingAI.Config.MaxAttackConfigurationsToExplore;
		}
		List<BlockingConfiguration> list = new List<BlockingConfiguration>();
		ProduceBlockConfigurations(initialBlockConfiguration, list, num);
		return GetSortedBlockConfigurations(cardDatabase, gameState, list);
	}

	public static List<BlockingConfiguration> GetSortedBlockConfigurations(ICardDatabaseAdapter cardDatabase, SimpleGameStateConstruction gameState, List<BlockingConfiguration> allBlockConfigurations)
	{
		List<BlockingConfiguration> list = new List<BlockingConfiguration>();
		foreach (BlockingConfiguration allBlockConfiguration in allBlockConfigurations)
		{
			if (StopExploring)
			{
				break;
			}
			if (allBlockConfiguration.IsValidBlockingConfiguration())
			{
				allBlockConfiguration.Score = OrderingBlockerAI.GetBestScoreOffProposedBlocks(gameState, allBlockConfiguration, cardDatabase);
				list.Add(allBlockConfiguration);
			}
		}
		if (list.Count == 0)
		{
			list.Add(allBlockConfigurations[0]);
		}
		else
		{
			list.Sort((BlockingConfiguration blockConfigA, BlockingConfiguration blockConfigB) => blockConfigB.Score.CompareTo(blockConfigA.Score));
		}
		return list;
	}

	public static void CalculateSortedBlockConfigurations(SimpleGameStateConstruction gameState, List<BlockingConfiguration> allBlockConfigurations, ICardDatabaseAdapter cardDatabase)
	{
		SortedBlockConfigurations.Clear();
		foreach (BlockingConfiguration allBlockConfiguration in allBlockConfigurations)
		{
			if (StopExploring)
			{
				break;
			}
			if (allBlockConfiguration.IsValidBlockingConfiguration())
			{
				allBlockConfiguration.Score = OrderingBlockerAI.GetBestScoreOffProposedBlocks(gameState, allBlockConfiguration, cardDatabase);
				SortedBlockConfigurations.Add(allBlockConfiguration);
			}
		}
		SortedBlockConfigurations.Sort((BlockingConfiguration blockConfigA, BlockingConfiguration blockConfigB) => blockConfigB.Score.CompareTo(blockConfigA.Score));
	}

	private static void Thread_RunBlockingAI(DeckHeuristic deckHeuristic, MtgGameState gameState, BlockingConfiguration initialBlockConfiguraion, ICardDatabaseAdapter cardDatabase)
	{
		ThreadTimer.Reset();
		ThreadTimer.Start();
		IsCalculating = true;
		List<BlockingConfiguration> list = new List<BlockingConfiguration>();
		SimpleGameStateConstruction gameState2 = new SimpleGameStateConstruction(deckHeuristic, gameState.LocalPlayer.InstanceId, gameState, cardDatabase.AbilityDataProvider);
		ProduceBlockConfigurations(initialBlockConfiguraion, list, Config.MaxBlockConfigurationsToExplore);
		CalculateSortedBlockConfigurations(gameState2, list, cardDatabase);
		IsCalculating = false;
		ThreadTimer.Stop();
	}

	public static void StartThread(DeckHeuristic deckHeuristic, MtgGameState gameState, BlockingConfiguration initialBlockConfiguration, ICardDatabaseAdapter cardDatabase)
	{
		if (_blockAIThread != null && _blockAIThread.IsAlive)
		{
			_blockAIThread.Abort();
		}
		StopExploring = false;
		_blockAIThread = new Thread((ThreadStart)delegate
		{
			Thread_RunBlockingAI(deckHeuristic, gameState, initialBlockConfiguration, cardDatabase);
		});
		_blockAIThread.Start();
	}

	public static bool StopThread()
	{
		bool result = true;
		if (_blockAIThread != null && _blockAIThread.IsAlive)
		{
			StopExploring = true;
			result = false;
		}
		_blockAIThread = null;
		return result;
	}
}
