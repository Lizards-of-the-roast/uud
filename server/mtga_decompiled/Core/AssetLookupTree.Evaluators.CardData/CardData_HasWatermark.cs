using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasWatermark : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, !string.IsNullOrWhiteSpace(bb.CardData.Printing.Watermark));
		}
		return false;
	}
}
