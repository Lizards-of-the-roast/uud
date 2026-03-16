using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Player;

public class Player_HasLostControl : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Player != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.Player.InstanceId != bb.Player.ControllerId);
		}
		return false;
	}
}
