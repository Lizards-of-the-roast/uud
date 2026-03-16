using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

public static class NPEStrategyHelpers
{
	private static void AddAPick(MtgGameState gameState, DeckHeuristic aiConfig, List<uint> ids, List<uint> topPicksList, List<uint> selections)
	{
		uint? num = null;
		foreach (uint id in ids)
		{
			if (id == gameState.Opponent.InstanceId)
			{
				num = id;
				break;
			}
		}
		if (num.HasValue)
		{
			selections.Add(num.Value);
			ids.Remove(num.Value);
			return;
		}
		uint? num2 = null;
		foreach (uint id2 in ids)
		{
			MtgCardInstance cardById = gameState.GetCardById(id2);
			if (cardById != null && cardById.ObjectType == GameObjectType.Card && topPicksList.Contains(cardById.GrpId))
			{
				num2 = id2;
				break;
			}
		}
		if (num2.HasValue)
		{
			selections.Add(num2.Value);
			ids.Remove(num2.Value);
			return;
		}
		float num3 = float.MinValue;
		uint? num4 = null;
		foreach (uint id3 in ids)
		{
			MtgCardInstance cardById2 = gameState.GetCardById(id3);
			if (cardById2 != null && cardById2.ObjectType == GameObjectType.Card)
			{
				float num5 = aiConfig.ScoreCard(cardById2);
				if (num5 > num3)
				{
					num4 = id3;
					num3 = num5;
				}
			}
		}
		if (num4.HasValue)
		{
			selections.Add(num4.Value);
			ids.Remove(num4.Value);
		}
	}

	public static List<uint> PickWell(MtgGameState gameState, DeckHeuristic aiConfig, List<uint> ids, int minPicks, uint maxPicks, List<uint> topPicksList, Random random)
	{
		int num = random.Next(minPicks, (int)(maxPicks + 1));
		List<uint> list = new List<uint>();
		while (list.Count < num)
		{
			AddAPick(gameState, aiConfig, ids, topPicksList, list);
		}
		return list;
	}
}
