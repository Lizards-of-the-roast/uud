using AssetLookupTree.Blackboard;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Qualification;

public class Qualification_SubType : EvaluatorBase_List<QualificationSubtype>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.Qualification.HasValue)
		{
			return EvaluatorBase_List<QualificationSubtype>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.Qualification.Value.SubType);
		}
		return false;
	}
}
