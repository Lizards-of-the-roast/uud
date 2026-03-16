using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_ControllerIsActivePlayer : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb?.GameState?.ActivePlayer != null && bb.CardData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.GameState.ActivePlayer.ClientPlayerEnum == bb.CardData.ControllerNum);
		}
		return false;
	}
}
