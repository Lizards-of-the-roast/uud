using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState.GameInfo;

public class GameState_GameInfo_MatchState : EvaluatorBase_List<MatchState>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState?.GameInfo != null)
		{
			return EvaluatorBase_List<MatchState>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GameState.GameInfo.MatchState);
		}
		return false;
	}
}
