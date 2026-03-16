using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_CardType : EvaluatorBase_List<CardType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<CardType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.CardTypes, MinCount, MaxCount);
		}
		return false;
	}
}
