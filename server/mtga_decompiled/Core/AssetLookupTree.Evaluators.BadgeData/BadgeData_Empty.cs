using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Ability.Metadata;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_Empty : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.BadgeData != null && bb.BadgeData.CompareTo(BadgeEntryData.Empty) == 0);
	}
}
