using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class FieldFillerType : EvaluatorBase_List<CDCFieldFillerFieldType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<CDCFieldFillerFieldType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.FieldFillerType);
	}
}
