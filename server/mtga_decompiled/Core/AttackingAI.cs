using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

public static class AttackingAI
{
	public static AttackConfig Config = DefaultConfig;

	private static Thread _attackAIThread;

	public static AttackConfig DefaultConfig => new AttackConfig(32u, 6f, 8f, 8f, 3f);

	public static List<AttackingConfiguration> SortedAttackConfigurations { get; private set; } = new List<AttackingConfiguration>();

	public static Stopwatch ThreadTimer { get; private set; } = new Stopwatch();

	public static bool StopExploring { get; private set; } = false;

	public static bool IsCalculating { get; private set; } = false;

	private static void ProduceAttackConfigurations(AttackingConfiguration currentConfiguration, List<AttackingConfiguration> allAttackConfigurations, HashSet<ulong> hashedAttackConfigurations, Dictionary<uint, uint> hashedAttackerStats, MtgGameState gameState, uint maxAttackConfigurationsToExplore = uint.MaxValue)
	{
		if (allAttackConfigurations.Count >= maxAttackConfigurationsToExplore || StopExploring)
		{
			if (allAttackConfigurations.Count == 0 && currentConfiguration.TryAddHashedAttackConfiguration(hashedAttackConfigurations, hashedAttackerStats, gameState))
			{
				allAttackConfigurations.Add(currentConfiguration);
			}
			return;
		}
		if (currentConfiguration.AbleAttackerIds.Count() == 0)
		{
			if (currentConfiguration.TryAddHashedAttackConfiguration(hashedAttackConfigurations, hashedAttackerStats, gameState))
			{
				allAttackConfigurations.Add(currentConfiguration);
			}
			if (allAttackConfigurations.Count < maxAttackConfigurationsToExplore)
			{
				_ = StopExploring;
			}
			return;
		}
		uint item = currentConfiguration.AbleAttackerIds.First();
		AttackingConfiguration attackingConfiguration = new AttackingConfiguration(currentConfiguration);
		attackingConfiguration.AbleAttackerIds.Remove(item);
		ProduceAttackConfigurations(attackingConfiguration, allAttackConfigurations, hashedAttackConfigurations, hashedAttackerStats, gameState, maxAttackConfigurationsToExplore);
		if (allAttackConfigurations.Count < maxAttackConfigurationsToExplore && !StopExploring)
		{
			AttackingConfiguration attackingConfiguration2 = new AttackingConfiguration(currentConfiguration);
			attackingConfiguration2.CommittedAttackerIds.Add(item);
			attackingConfiguration2.AbleAttackerIds.Remove(item);
			ProduceAttackConfigurations(attackingConfiguration2, allAttackConfigurations, hashedAttackConfigurations, hashedAttackerStats, gameState, maxAttackConfigurationsToExplore);
		}
	}

	public static List<uint> GetBestAttackConfiguration(DeckHeuristic deckHeuristic, MtgGameState gameState, DeclareAttackerRequest request, ICardDatabaseAdapter cardDatabase)
	{
		return GetSortedAttackConfigurations(deckHeuristic, gameState, request, cardDatabase)[0].CommittedAttackerIds;
	}

	public static List<AttackingConfiguration> GetSortedAttackConfigurations(DeckHeuristic deckHeuristic, MtgGameState gameState, DeclareAttackerRequest request, ICardDatabaseAdapter cardDatabase)
	{
		return GetSortedAttackConfigurations(deckHeuristic, gameState, new AttackingConfiguration(request), cardDatabase);
	}

	public static List<AttackingConfiguration> GetSortedAttackConfigurations(DeckHeuristic deckHeuristic, MtgGameState gameState, AttackingConfiguration initialAttackConfiguration, ICardDatabaseAdapter cardDatabase)
	{
		List<AttackingConfiguration> list = new List<AttackingConfiguration>();
		HashSet<ulong> hashedAttackConfigurations = new HashSet<ulong>();
		Dictionary<uint, uint> hashedAttackerStats = new Dictionary<uint, uint>();
		List<AttackingConfiguration> list2 = new List<AttackingConfiguration>();
		ProduceAttackConfigurations(initialAttackConfiguration, list2, hashedAttackConfigurations, hashedAttackerStats, gameState, Config.MaxAttackConfigurationsToExplore);
		foreach (AttackingConfiguration item in list2)
		{
			if (StopExploring)
			{
				break;
			}
			BlockingConfiguration initialBlockingConfiguration = new BlockingConfiguration(gameState, item);
			SimpleGameStateConstruction gameState2 = new SimpleGameStateConstruction(deckHeuristic, gameState.Opponent.InstanceId, gameState, cardDatabase.AbilityDataProvider);
			item.Score = BlockingAI.GetBestBlockConfiguration(gameState2, initialBlockingConfiguration, cardDatabase).Score;
			list.Add(item);
		}
		list.Sort((AttackingConfiguration attackConfigA, AttackingConfiguration attackConfigB) => attackConfigA.Score.CompareTo(attackConfigB.Score));
		return list;
	}

	public static void CalculateSortedAttackConfigurations(DeckHeuristic deckHeuristic, MtgGameState gameState, List<AttackingConfiguration> allAttackConfigurations, ICardDatabaseAdapter cardDatabase)
	{
		SortedAttackConfigurations.Clear();
		foreach (AttackingConfiguration allAttackConfiguration in allAttackConfigurations)
		{
			if (StopExploring)
			{
				break;
			}
			BlockingConfiguration initialBlockingConfiguration = new BlockingConfiguration(gameState, allAttackConfiguration);
			SimpleGameStateConstruction gameState2 = new SimpleGameStateConstruction(deckHeuristic, gameState.Opponent.InstanceId, gameState, cardDatabase.AbilityDataProvider);
			allAttackConfiguration.Score = BlockingAI.GetBestBlockConfiguration(gameState2, initialBlockingConfiguration, cardDatabase).Score;
			allAttackConfiguration.Score -= (float)allAttackConfiguration.CommittedAttackerIds.Count * deckHeuristic._aggressionPerAttackerWeight;
			SortedAttackConfigurations.Add(allAttackConfiguration);
		}
		if (SortedAttackConfigurations.Count == 0)
		{
			SortedAttackConfigurations.Add(allAttackConfigurations[0]);
			return;
		}
		SortedAttackConfigurations.Sort((AttackingConfiguration attackConfigA, AttackingConfiguration attackConfigB) => attackConfigA.Score.CompareTo(attackConfigB.Score));
	}

	private static void Thread_RunAttackingAI(DeckHeuristic deckHeuristic, MtgGameState gameState, AttackingConfiguration initialAttackingConfiguration, ICardDatabaseAdapter cardDatabase)
	{
		ThreadTimer.Reset();
		IsCalculating = true;
		ThreadTimer.Start();
		List<AttackingConfiguration> allAttackConfigurations = new List<AttackingConfiguration>();
		HashSet<ulong> hashedAttackConfigurations = new HashSet<ulong>();
		Dictionary<uint, uint> hashedAttackerStats = new Dictionary<uint, uint>();
		ProduceAttackConfigurations(initialAttackingConfiguration, allAttackConfigurations, hashedAttackConfigurations, hashedAttackerStats, gameState, BlockingAI.Config.MaxBlockConfigurationsToExplore);
		CalculateSortedAttackConfigurations(deckHeuristic, gameState, allAttackConfigurations, cardDatabase);
		ThreadTimer.Stop();
		IsCalculating = false;
	}

	public static void StartThread(DeckHeuristic deckHeuristic, MtgGameState gameState, AttackingConfiguration initialAttackConfiguration, ICardDatabaseAdapter cardDatabase)
	{
		if (_attackAIThread != null && _attackAIThread.IsAlive)
		{
			_attackAIThread.Abort();
		}
		StopExploring = false;
		_attackAIThread = new Thread((ThreadStart)delegate
		{
			Thread_RunAttackingAI(deckHeuristic, gameState, initialAttackConfiguration, cardDatabase);
		});
		_attackAIThread.Start();
	}

	public static bool StopThread()
	{
		bool result = true;
		if (_attackAIThread != null && _attackAIThread.IsAlive)
		{
			StopExploring = true;
			result = false;
		}
		_attackAIThread = null;
		return result;
	}
}
