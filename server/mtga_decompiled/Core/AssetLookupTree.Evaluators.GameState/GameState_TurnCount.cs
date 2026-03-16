using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.GameState;

public class GameState_TurnCount : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.GameState != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.GameState.GameWideTurn);
		}
		return false;
	}
}
