using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_OwnerIsActivePlayer : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState?.ActivePlayer != null && bb.CardData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.GameState.ActivePlayer.ClientPlayerEnum == bb.CardData.OwnerNum);
		}
		return false;
	}
}
