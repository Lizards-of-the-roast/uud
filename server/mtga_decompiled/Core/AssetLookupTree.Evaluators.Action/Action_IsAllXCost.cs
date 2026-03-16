using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Action;

public class Action_IsAllXCost : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.GreAction != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.GreAction.AreAllManaCostsColorX());
		}
		return false;
	}
}
