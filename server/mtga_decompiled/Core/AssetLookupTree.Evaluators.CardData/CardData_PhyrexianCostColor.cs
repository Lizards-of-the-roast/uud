using System;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_PhyrexianCostColor : EvaluatorBase_List<ManaColor>
{
	public override bool Execute(IBlackboard bb)
	{
		ManaQuantity manaQuantity = ManaQuantity.Empty;
		foreach (ManaQuantity item in bb.CardData.PrintedCastingCost)
		{
			if (item.IsPhyrexian)
			{
				manaQuantity = item;
				break;
			}
		}
		if (manaQuantity.Equals(ManaQuantity.Empty))
		{
			foreach (AbilityPrintingData ability in bb.CardData.Abilities)
			{
				foreach (ManaQuantity item2 in ability.ManaCost)
				{
					if (item2.IsPhyrexian)
					{
						manaQuantity = item2;
						break;
					}
				}
			}
		}
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<ManaColor>.GetResult(ExpectedValues, Operation, ExpectedResult, manaQuantity.Colors ?? Array.Empty<ManaColor>(), MinCount, MaxCount);
		}
		return false;
	}
}
