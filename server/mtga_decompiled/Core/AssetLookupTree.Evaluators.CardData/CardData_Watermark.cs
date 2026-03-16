using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Watermark : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrWhiteSpace(bb.CardData?.Printing?.Watermark))
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.CardData.Printing.Watermark);
		}
		return false;
	}
}
