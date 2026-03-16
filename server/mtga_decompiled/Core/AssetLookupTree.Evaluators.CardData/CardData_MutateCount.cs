using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_MutateCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		int? num = bb.CardData.Instance?.MutationChildren.Count;
		int inValue = 0;
		if (num.HasValue)
		{
			inValue = num.Value;
		}
		if (bb.CardData != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, inValue);
		}
		return false;
	}
}
