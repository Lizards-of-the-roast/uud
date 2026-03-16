using AssetLookupTree.Blackboard;
using AssetLookupTree.Payloads.Ability.Metadata;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_Category : EvaluatorBase_List<BadgeEntryCategory>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.BadgeData != null)
		{
			return EvaluatorBase_List<BadgeEntryCategory>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.BadgeData.Category);
		}
		return false;
	}
}
