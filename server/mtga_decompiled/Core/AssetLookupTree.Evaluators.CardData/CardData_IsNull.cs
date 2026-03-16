using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_IsNull : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData == null);
	}
}
