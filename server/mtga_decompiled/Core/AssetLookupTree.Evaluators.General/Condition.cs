using AssetLookupTree.Blackboard;
using Wotc.Mtga.Hangers;

namespace AssetLookupTree.Evaluators.General;

public class Condition : EvaluatorBase_List<ConditionType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Condition != ConditionType.None)
		{
			return EvaluatorBase_List<ConditionType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Condition);
		}
		return false;
	}
}
