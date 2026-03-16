using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.ManaMovement;

public class ManaMovement_SubstitutionGrpId : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ManaMovement.IsValid)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, (int)bb.ManaMovement.SubstitutionGrpId);
		}
		return false;
	}
}
