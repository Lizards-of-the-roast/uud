using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_SuperType : EvaluatorBase_List<SuperType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<SuperType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Supertypes, MinCount, MaxCount);
		}
		return false;
	}
}
