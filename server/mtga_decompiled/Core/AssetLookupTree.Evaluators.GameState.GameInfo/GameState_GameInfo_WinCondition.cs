using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState.GameInfo;

public class GameState_GameInfo_WinCondition : EvaluatorBase_List<MatchWinCondition>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState?.GameInfo != null)
		{
			return EvaluatorBase_List<MatchWinCondition>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GameState.GameInfo.MatchWinCondition);
		}
		return false;
	}
}
