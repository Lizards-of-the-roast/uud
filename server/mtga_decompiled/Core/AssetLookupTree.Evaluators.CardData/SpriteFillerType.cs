using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class SpriteFillerType : EvaluatorBase_List<CDCSpriteFiller.FieldType>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<CDCSpriteFiller.FieldType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.SpriteFillerType);
	}
}
