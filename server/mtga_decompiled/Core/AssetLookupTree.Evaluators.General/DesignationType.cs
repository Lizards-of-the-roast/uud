using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.General;

public class DesignationType : EvaluatorBase_List<Designation>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Designation.HasValue)
		{
			return EvaluatorBase_List<Designation>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Designation.Value);
		}
		return false;
	}
}
