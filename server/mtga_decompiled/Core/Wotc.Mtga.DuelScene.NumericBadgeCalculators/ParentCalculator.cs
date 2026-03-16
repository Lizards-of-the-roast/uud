using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public abstract class ParentCalculator : INumericBadgeCalculator
{
	protected readonly INumericBadgeCalculator _childCalculator;

	public ParentCalculator(INumericBadgeCalculator childCalculator)
	{
		_childCalculator = childCalculator;
	}

	protected abstract void SetupChild();

	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		SetupChild();
		MtgCardInstance parent = input.CardData.Parent;
		if (parent != null)
		{
			return _childCalculator.HasNumber(new NumericBadgeCalculatorInput
			{
				CardData = new CardData(parent, input.CardData.Printing),
				Ability = input.Ability,
				GameState = input.GameState
			});
		}
		return false;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		SetupChild();
		MtgCardInstance parent = input.CardData.Parent;
		if (parent != null)
		{
			return _childCalculator.GetNumber(new NumericBadgeCalculatorInput
			{
				CardData = new CardData(parent, input.CardData.Printing),
				Ability = input.Ability,
				GameState = input.GameState
			}, out number, out modifier);
		}
		number = 0;
		modifier = string.Empty;
		return false;
	}
}
