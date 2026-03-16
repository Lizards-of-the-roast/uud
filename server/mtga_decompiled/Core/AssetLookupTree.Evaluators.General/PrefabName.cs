using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class PrefabName : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrWhiteSpace(bb.PrefabName))
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.PrefabName);
		}
		return false;
	}
}
