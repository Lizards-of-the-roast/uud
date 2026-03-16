using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.ManaMovement;

public class ManaMovement_Color : EvaluatorBase_List<ManaColor>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.ManaMovement.IsValid)
		{
			return EvaluatorBase_List<ManaColor>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ManaMovement.Color);
		}
		return false;
	}
}
