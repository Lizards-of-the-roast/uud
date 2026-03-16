using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_BaseId : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.Ability.BaseId);
		}
		return false;
	}
}
