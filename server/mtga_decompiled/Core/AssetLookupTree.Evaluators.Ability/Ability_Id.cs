using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_Id : EvaluatorBase_Int
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Ability != null)
		{
			return EvaluatorBase_Int.GetResult(MinExpectedResult, MaxExpectedResult, Operation, ExpectedResult, (int)bb.Ability.Id);
		}
		return false;
	}
}
