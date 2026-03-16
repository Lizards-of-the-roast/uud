using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Action;

public class Action_Visibility : EvaluatorBase_List<Visibility>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.GreAction != null)
		{
			return EvaluatorBase_List<Visibility>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GreAction.Visibility);
		}
		return false;
	}
}
