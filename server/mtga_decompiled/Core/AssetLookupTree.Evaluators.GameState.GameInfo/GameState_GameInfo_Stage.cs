using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState.GameInfo;

public class GameState_GameInfo_Stage : EvaluatorBase_List<GameStage>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState?.GameInfo != null)
		{
			return EvaluatorBase_List<GameStage>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GameState.GameInfo.Stage);
		}
		return false;
	}
}
