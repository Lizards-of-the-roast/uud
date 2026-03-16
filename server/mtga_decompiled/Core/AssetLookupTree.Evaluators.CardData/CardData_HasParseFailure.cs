using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasParseFailure : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Printing != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Printing.HasParseFailure);
		}
		return false;
	}
}
