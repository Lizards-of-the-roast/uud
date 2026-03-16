using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState;

public class GameState_Step : EvaluatorBase_List<Step>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState != null)
		{
			return EvaluatorBase_List<Step>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GameState.CurrentStep);
		}
		return false;
	}
}
