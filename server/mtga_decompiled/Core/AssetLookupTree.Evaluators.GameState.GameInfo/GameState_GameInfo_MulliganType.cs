using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.GameState.GameInfo;

public class GameState_GameInfo_MulliganType : EvaluatorBase_List<MulliganType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState?.GameInfo != null)
		{
			return EvaluatorBase_List<MulliganType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.GameState.GameInfo.MulliganType);
		}
		return false;
	}
}
