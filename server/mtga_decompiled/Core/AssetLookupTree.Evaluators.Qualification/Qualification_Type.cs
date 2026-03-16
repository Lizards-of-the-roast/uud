using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Qualification;

public class Qualification_Type : EvaluatorBase_List<QualificationType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Qualification.HasValue)
		{
			return EvaluatorBase_List<QualificationType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Qualification.Value.Type);
		}
		return false;
	}
}
