using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Player;

public class Player_LifeTotalVsStartingLifeTotal : EvaluatorBase_IntToInt
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Player != null)
		{
			return EvaluatorBase_IntToInt.GetResult(ValueOneModifier, ValueTwoModifier, Operation, ExpectedResult, bb.Player.LifeTotal, (int)bb.Player.StartingLifeTotal);
		}
		return false;
	}
}
