using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_othersideGrpId : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		uint? num = bb.CardData?.Instance?.OthersideGrpId;
		if (num.HasValue)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)num.Value);
		}
		return false;
	}
}
