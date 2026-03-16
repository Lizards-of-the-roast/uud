using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Color : EvaluatorBase_List<CardColor>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<CardColor>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Colors, MinCount, MaxCount);
		}
		return false;
	}
}
