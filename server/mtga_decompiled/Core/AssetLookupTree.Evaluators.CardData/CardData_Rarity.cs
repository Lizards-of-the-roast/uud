using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Rarity : EvaluatorBase_List<CardRarity>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<CardRarity>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Rarity);
		}
		return false;
	}
}
