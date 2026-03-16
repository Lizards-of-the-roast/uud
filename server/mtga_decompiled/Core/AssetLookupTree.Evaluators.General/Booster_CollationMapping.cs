using AssetLookupTree.Blackboard;
using Wotc.Mtga.Wrapper;

namespace AssetLookupTree.Evaluators.General;

public class Booster_CollationMapping : EvaluatorBase_List<CollationMapping>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<CollationMapping>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.BoosterCollationMapping);
	}
}
