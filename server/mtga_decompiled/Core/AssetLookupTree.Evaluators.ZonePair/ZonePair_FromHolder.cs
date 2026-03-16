using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.ZonePair;

public class ZonePair_FromHolder : EvaluatorBase_List<CardHolderType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<CardHolderType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.ZonePair.FromHolder);
	}
}
