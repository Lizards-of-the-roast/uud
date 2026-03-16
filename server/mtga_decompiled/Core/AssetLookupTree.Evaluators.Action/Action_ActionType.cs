using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Action;

public class Action_ActionType : EvaluatorBase_List<ActionType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.GreAction != null)
		{
			return EvaluatorBase_List<ActionType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GreAction.ActionType);
		}
		return false;
	}
}
