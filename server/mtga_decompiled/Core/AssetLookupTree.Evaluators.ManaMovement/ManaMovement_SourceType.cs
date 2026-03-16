using AssetLookupTree.Blackboard;
using Wotc.Mtga.Duel;

namespace AssetLookupTree.Evaluators.ManaMovement;

public class ManaMovement_SourceType : EvaluatorBase_List<ManaMovementData.ResourceSourceType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ManaMovement.IsValid)
		{
			return EvaluatorBase_List<ManaMovementData.ResourceSourceType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ManaMovement.SourceType);
		}
		return false;
	}
}
