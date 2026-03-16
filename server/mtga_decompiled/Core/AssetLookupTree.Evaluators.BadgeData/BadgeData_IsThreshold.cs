using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_IsThreshold : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.BadgeData == null)
		{
			return false;
		}
		if (bb.BadgeData.NumberCalculator is IThresholdBadgeCalculator thresholdBadgeCalculator)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, thresholdBadgeCalculator.HasThreshold(new NumericBadgeCalculatorInput
			{
				Ability = bb.Ability,
				CardData = bb.CardData,
				GameState = bb.GameState
			}));
		}
		return false;
	}
}
