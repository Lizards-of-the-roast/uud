using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.General;

public class GREActionType : EvaluatorBase_List<ActionType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.GreActionType != ActionType.None)
		{
			return EvaluatorBase_List<ActionType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GreActionType);
		}
		return false;
	}
}
