using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.ManaMovement;

public class ManaMovement_IsFueling : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ManaMovement.IsValid)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.ManaMovement.IsFueling);
		}
		return false;
	}
}
