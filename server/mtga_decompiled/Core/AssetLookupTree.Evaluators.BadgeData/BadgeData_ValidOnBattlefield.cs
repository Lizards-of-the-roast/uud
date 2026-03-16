using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_ValidOnBattlefield : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.BadgeData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.BadgeData.ValidOnBattlefield);
		}
		return false;
	}
}
