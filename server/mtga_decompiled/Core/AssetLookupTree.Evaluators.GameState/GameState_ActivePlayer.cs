using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.GameState;

public class GameState_ActivePlayer : EvaluatorBase_List<GREPlayerNum>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState?.ActivePlayer != null)
		{
			return EvaluatorBase_List<GREPlayerNum>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GameState.ActivePlayer.ClientPlayerEnum);
		}
		return false;
	}
}
