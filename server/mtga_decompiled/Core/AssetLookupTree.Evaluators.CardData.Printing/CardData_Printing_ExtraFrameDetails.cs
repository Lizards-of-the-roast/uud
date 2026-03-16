using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_ExtraFrameDetails : EvaluatorBase_List<ExtraFrameDetailType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_List<ExtraFrameDetailType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing.ExtraFrameDetails.Keys, MinCount, MaxCount);
		}
		return false;
	}
}
