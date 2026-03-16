using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_RawFrameDetails : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.CardData.Printing.RawFrameDetails);
		}
		return false;
	}
}
