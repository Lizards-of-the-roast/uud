using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.CardData;

public class Material_Name : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrWhiteSpace(bb.MaterialName))
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.MaterialName);
		}
		if ((bool)bb.Material)
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.Material.name);
		}
		return false;
	}
}
