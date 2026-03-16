using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.GameState;

public class GameState_NextExtraTurnPlayer : EvaluatorBase_List<GREPlayerNum>
{
	public override bool Execute(IBlackboard bb)
	{
		MtgGameState gameState = bb.GameState;
		if (gameState != null)
		{
			return EvaluatorBase_List<GREPlayerNum>.GetResult(ExpectedValues, Operation, ExpectedResult, gameState.ExtraTurns.Select((ExtraTurn x) => x.ActivePlayer), MinCount, MaxCount);
		}
		return false;
	}
}
