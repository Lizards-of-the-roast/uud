using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_IsTemporary : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null && bb.CardData.Instance != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Instance.IsTemporary);
		}
		return false;
	}
}
