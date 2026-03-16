using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.General;

public class LayeredEffectType : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		if (!string.IsNullOrWhiteSpace(bb.LayeredEffectType))
		{
			return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.LayeredEffectType);
		}
		return false;
	}
}
