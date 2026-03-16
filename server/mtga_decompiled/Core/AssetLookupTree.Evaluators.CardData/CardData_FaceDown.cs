using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_FaceDown : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.CardData.Instance.FaceDownState.IsFaceDown);
		}
		return false;
	}
}
