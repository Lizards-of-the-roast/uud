using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.NumericBadgeCalculators;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_IsNumeric : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.BadgeData != null)
		{
			return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.BadgeData.NumberCalculator.HasNumber(new NumericBadgeCalculatorInput
			{
				Ability = bb.Ability,
				CardData = bb.CardData,
				GameState = bb.GameState
			}));
		}
		return false;
	}
}
