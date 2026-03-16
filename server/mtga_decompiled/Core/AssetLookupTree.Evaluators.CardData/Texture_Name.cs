using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class Texture_Name : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrWhiteSpace(bb.TextureName))
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.TextureName);
		}
		if ((bool)bb.Texture)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.Texture.name);
		}
		return false;
	}
}
