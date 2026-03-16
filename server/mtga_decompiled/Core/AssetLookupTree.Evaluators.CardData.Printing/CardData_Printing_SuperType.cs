using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_SuperType : EvaluatorBase_List<SuperType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_List<SuperType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing.Supertypes, MinCount, MaxCount);
		}
		return false;
	}
}
