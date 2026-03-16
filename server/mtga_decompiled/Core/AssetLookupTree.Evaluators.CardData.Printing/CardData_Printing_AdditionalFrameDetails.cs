using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_AdditionalFrameDetails : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing.AdditionalFrameDetails, MinCount, MaxCount);
		}
		return false;
	}
}
