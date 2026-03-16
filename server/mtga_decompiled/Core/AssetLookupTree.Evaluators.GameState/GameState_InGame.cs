using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.GameState;

public class GameState_InGame : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.GameState != null);
	}
}
