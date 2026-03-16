using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class Font_Name : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrWhiteSpace(bb.FontName))
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.FontName);
		}
		if ((bool)bb.Font)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.Font.name);
		}
		return false;
	}
}
