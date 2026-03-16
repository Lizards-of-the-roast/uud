using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public abstract class ThresholdParentCalculator : ParentCalculator, IThresholdBadgeCalculator
{
	protected readonly IThresholdBadgeCalculator _childThresholdCalculator;

	public ThresholdBadgeMode BadgeMode { get; set; } = ThresholdBadgeMode.Count;

	protected ThresholdParentCalculator(IThresholdBadgeCalculator childThresholdCalc, INumericBadgeCalculator childCalculator)
		: base(childCalculator)
	{
		_childThresholdCalculator = childThresholdCalc;
		BadgeMode = childThresholdCalc.BadgeMode;
	}

	public bool GetThreshold(NumericBadgeCalculatorInput input, out int threshold)
	{
		SetupChild();
		MtgCardInstance parent = input.CardData.Parent;
		if (parent != null)
		{
			return _childThresholdCalculator.GetThreshold(new NumericBadgeCalculatorInput
			{
				CardData = new CardData(parent, input.CardData.Printing),
				Ability = input.Ability,
				GameState = input.GameState
			}, out threshold);
		}
		threshold = 0;
		return false;
	}

	public bool HasThreshold(NumericBadgeCalculatorInput input)
	{
		SetupChild();
		MtgCardInstance parent = input.CardData.Parent;
		if (parent != null)
		{
			return _childThresholdCalculator.HasThreshold(new NumericBadgeCalculatorInput
			{
				CardData = new CardData(parent, input.CardData.Printing),
				Ability = input.Ability,
				GameState = input.GameState
			});
		}
		return false;
	}
}
