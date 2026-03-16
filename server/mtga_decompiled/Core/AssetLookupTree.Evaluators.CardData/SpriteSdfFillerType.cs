using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class SpriteSdfFillerType : EvaluatorBase_List<CDCSpriteFillerSDF.FieldType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<CDCSpriteFillerSDF.FieldType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.SpriteSdfFillerType);
	}
}
