using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState.GameInfo;

public class GameState_GameInfo_Type : EvaluatorBase_List<GameType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState?.GameInfo != null)
		{
			return EvaluatorBase_List<GameType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GameState.GameInfo.Type);
		}
		return false;
	}
}
