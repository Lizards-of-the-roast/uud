using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Player;

public class Player_CompanionId : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Player != null && bb.Player.CompanionId != 0)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)bb.Player.CompanionId);
		}
		return false;
	}
}
