using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState;

public class GameState_NextPhase : EvaluatorBase_List<Phase>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState != null)
		{
			return EvaluatorBase_List<Phase>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GameState.NextPhase);
		}
		return false;
	}
}
