using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_LinkedInstanceCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Instance?.LinkedFaceInstances != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, bb.CardData.Instance.LinkedFaceInstances.Count);
		}
		return false;
	}
}
