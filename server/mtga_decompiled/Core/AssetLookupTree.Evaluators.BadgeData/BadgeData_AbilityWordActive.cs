using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_AbilityWordActive : EvaluatorBase_String
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_String.GetResult(ExpectedValue, Operation, ExpectedResult, bb.BadgeData?.GetActivationWord() ?? string.Empty);
	}
}
