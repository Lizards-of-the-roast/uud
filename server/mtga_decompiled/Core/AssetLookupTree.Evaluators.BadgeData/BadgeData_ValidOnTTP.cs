using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_ValidOnTTP : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.BadgeData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.BadgeData.ValidOnTTP);
		}
		return false;
	}
}
