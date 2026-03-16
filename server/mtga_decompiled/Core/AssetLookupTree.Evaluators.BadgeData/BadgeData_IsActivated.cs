using AssetLookupTree.Blackboard;
using Wotc.Mtga.DuelScene.BadgeActivationCalculators;

namespace AssetLookupTree.Evaluators.BadgeData;

public class BadgeData_IsActivated : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.BadgeData == null || bb.BadgeData.ActivationCalculator == null)
		{
			return false;
		}
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, bb.BadgeData.ActivationCalculator.GetActive(new BadgeActivationCalculatorInput(bb.CardData, bb.Ability, bb.GameState)));
	}
}
