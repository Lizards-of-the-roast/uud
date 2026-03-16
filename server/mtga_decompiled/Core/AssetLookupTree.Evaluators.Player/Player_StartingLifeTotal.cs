using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Player;

public class Player_StartingLifeTotal : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Player != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.Player.StartingLifeTotal);
		}
		return false;
	}
}
