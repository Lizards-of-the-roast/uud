using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_IsAttached : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Instance.AttachedToId != 0);
		}
		return false;
	}
}
