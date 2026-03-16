using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasAbilitiesWithTag : EvaluatorBase_List<MetaDataTag>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<MetaDataTag>.GetResult(ExpectedValues, Operation, ExpectedResult, GetTags(bb.CardData), MinCount, MaxCount);
	}

	private IEnumerable<MetaDataTag> GetTags(ICardDataAdapter cardData)
	{
		foreach (AbilityPrintingData item in cardData?.Abilities)
		{
			foreach (MetaDataTag item2 in item?.Tags)
			{
				yield return item2;
			}
		}
	}
}
