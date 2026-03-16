using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.GameState;

public class GameState_DecidingPlayer : EvaluatorBase_List<GREPlayerNum>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb == null)
		{
			return false;
		}
		MtgGameState gameState = bb.GameState;
		if (gameState == null)
		{
			return false;
		}
		MtgPlayer decidingPlayer = gameState.DecidingPlayer;
		if (decidingPlayer == null)
		{
			return false;
		}
		return EvaluatorBase_List<GREPlayerNum>.GetResult(ExpectedValues, Operation, ExpectedResult, decidingPlayer.ClientPlayerEnum);
	}
}
