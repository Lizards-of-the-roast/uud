using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_SubType : EvaluatorBase_List<SubType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_List<SubType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing.Subtypes, MinCount, MaxCount);
		}
		return false;
	}
}
