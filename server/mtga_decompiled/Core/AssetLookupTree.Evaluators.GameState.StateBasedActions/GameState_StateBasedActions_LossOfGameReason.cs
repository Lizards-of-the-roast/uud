using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.GameState.StateBasedActions;

public class GameState_StateBasedActions_LossOfGameReason : EvaluatorBase_List<LossOfGameReason>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb != null)
		{
			MtgGameState gameState = bb.GameState;
			if (gameState != null)
			{
				_ = gameState.GameLossData;
				if (true)
				{
					return EvaluatorBase_List<LossOfGameReason>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GameState.GameLossData.Reason);
				}
			}
		}
		return false;
	}
}
