using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_Threshold_BadgeMode : EvaluatorBase_List<ThresholdBadgeMode>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.BadgeData == null || !(bb.BadgeData.NumberCalculator is IThresholdBadgeCalculator thresholdBadgeCalculator))
		{
			return false;
		}
		return EvaluatorBase_List<ThresholdBadgeMode>.GetResult(ExpectedValues, Operation, ExpectedResult, thresholdBadgeCalculator.BadgeMode);
	}
}
