using AssetLookupTree.Blackboard;
using Wotc.Mtga.Duel;

namespace AssetLookupTree.Evaluators.ManaMovement;

public class ManaMovement_SinkType : EvaluatorBase_List<ManaMovementData.ResourceSinkType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ManaMovement.IsValid)
		{
			return EvaluatorBase_List<ManaMovementData.ResourceSinkType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ManaMovement.SinkType);
		}
		return false;
	}
}
