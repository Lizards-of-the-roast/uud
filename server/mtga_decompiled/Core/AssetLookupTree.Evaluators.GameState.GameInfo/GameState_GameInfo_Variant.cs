using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState.GameInfo;

public class GameState_GameInfo_Variant : EvaluatorBase_List<GameVariant>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState?.GameInfo != null)
		{
			return EvaluatorBase_List<GameVariant>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GameState.GameInfo.Variant);
		}
		return false;
	}
}
