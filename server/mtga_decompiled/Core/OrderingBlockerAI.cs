using System.Collections.Generic;
using System.Linq;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

public class OrderingBlockerAI
{
	public static IEnumerable<List<uint>> Permutate(List<uint> input)
	{
		if (input.Count == 0 || input.Count == 1)
		{
			yield return new List<uint>(input);
			yield break;
		}
		foreach (uint elem in input)
		{
			List<uint> list = new List<uint>(input);
			list.Remove(elem);
			foreach (List<uint> item in Permutate(list))
			{
				item.Insert(0, elem);
				yield return item;
			}
		}
	}

	private static Dictionary<string, object> CanWeCombatTrick(MtgGameState gamestate, uint trickTargetId, DeckHeuristic deckHeuristic, ICardDatabaseAdapter cardDatabase)
	{
		Dictionary<string, object> dictionary = null;
		foreach (MtgCardInstance visibleCard in gamestate.LocalHand.VisibleCards)
		{
			bool flag = false;
			CardPrintingData cardPrintingById = cardDatabase.CardDataProvider.GetCardPrintingById(visibleCard.GrpId);
			foreach (CardHeuristic cardHeuristic in deckHeuristic.GetCardHeuristics(cardPrintingById.TitleId, cardDatabase))
			{
				if (cardHeuristic.Archetype != Archetype.CombatTrick)
				{
					continue;
				}
				MtgEntity mtgEntity = null;
				if (gamestate.TryGetEntity(trickTargetId, out mtgEntity))
				{
					dictionary = cardHeuristic.CanCombatTrickBeApplied((MtgCardInstance)mtgEntity, gamestate, cardDatabase);
					if (dictionary != null)
					{
						flag = true;
						break;
					}
				}
			}
			if (flag)
			{
				break;
			}
		}
		return dictionary;
	}

	public static float GetBestScoreOffProposedBlocks(SimpleGameStateConstruction startingGameState, BlockingConfiguration blockingConfiguration, ICardDatabaseAdapter cardDatabase)
	{
		Dictionary<uint, FinalizedCombatPacket> dictionary = new Dictionary<uint, FinalizedCombatPacket>();
		DetermineBestOrderingsIndividually(startingGameState, blockingConfiguration, dictionary, cardDatabase);
		SimpleGameStateConstruction resultantGameStateFromFullConfiguration = startingGameState.GetResultantGameStateFromFullConfiguration(dictionary);
		float num = startingGameState.ScoreChange(resultantGameStateFromFullConfiguration);
		foreach (KeyValuePair<uint, FinalizedCombatPacket> item in dictionary)
		{
			if (AttackingAI.StopExploring || BlockingAI.StopExploring)
			{
				break;
			}
			if (item.Value.BlockerOrdering.Count > 1)
			{
				num -= startingGameState.DeckHeuristic._multiBlockPenaltyPerCreatureBeyondFirst * (float)(item.Value.BlockerOrdering.Count - 1);
			}
		}
		return num;
	}

	private static void DetermineBestOrderingsIndividually(SimpleGameStateConstruction startingGameState, BlockingConfiguration blockingConfiguration, Dictionary<uint, FinalizedCombatPacket> finalizedConfiguration, ICardDatabaseAdapter cardDatabase)
	{
		foreach (KeyValuePair<uint, HashSet<uint>> attackerIdToCoupledBlockerId in blockingConfiguration.AttackerIdToCoupledBlockerIds)
		{
			if (AttackingAI.StopExploring || BlockingAI.StopExploring)
			{
				break;
			}
			uint key = attackerIdToCoupledBlockerId.Key;
			List<uint> list = attackerIdToCoupledBlockerId.Value.ToList();
			List<List<uint>> list2 = Permutate(list).ToList();
			List<uint> list3 = list;
			float num = float.MaxValue;
			FinalizedCombatPacket finalizedCombatPacket = new FinalizedCombatPacket(key, list3);
			Dictionary<uint, Dictionary<string, object>> dictionary = new Dictionary<uint, Dictionary<string, object>>();
			Dictionary<string, object> dictionary2 = CanWeCombatTrick(startingGameState.StateForReference, key, startingGameState.DeckHeuristic, cardDatabase);
			if (dictionary2 != null)
			{
				dictionary.Add(key, dictionary2);
			}
			foreach (uint item in list3)
			{
				dictionary2 = CanWeCombatTrick(startingGameState.StateForReference, item, startingGameState.DeckHeuristic, cardDatabase);
				if (dictionary2 != null)
				{
					dictionary.Add(item, dictionary2);
				}
			}
			finalizedCombatPacket.CreatureToModificationsToBeApplied = dictionary;
			foreach (List<uint> item2 in list2)
			{
				if (AttackingAI.StopExploring || BlockingAI.StopExploring)
				{
					break;
				}
				SimpleGameStateConstruction resultantGameStateFromPotentialCombatPacket = startingGameState.GetResultantGameStateFromPotentialCombatPacket(key, item2, dictionary);
				float num2 = startingGameState.ScoreChange(resultantGameStateFromPotentialCombatPacket);
				if (num2 <= num)
				{
					num = num2;
					list3 = item2;
				}
			}
			finalizedCombatPacket.BlockerOrdering = list3;
			finalizedConfiguration[key] = finalizedCombatPacket;
		}
	}
}
