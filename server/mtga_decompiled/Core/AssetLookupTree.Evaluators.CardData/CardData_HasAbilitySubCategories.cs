using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasAbilitySubCategories : EvaluatorBase_List<AbilitySubCategory>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<AbilitySubCategory>.GetResult(ExpectedValues, Operation, ExpectedResult, from x in bb.CardData.Abilities
				where x.SubCategory != AbilitySubCategory.None
				select x.SubCategory, MinCount, MaxCount);
		}
		return false;
	}
}
