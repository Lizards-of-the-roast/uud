namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class MutationCount : INumericBadgeCalculator
{
	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		return (input.CardData?.Instance?.MutationChildren?.Count).GetValueOrDefault() > 0;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		int? num = input.CardData?.Instance?.MutationChildren.Count;
		modifier = null;
		if (num.HasValue && num.Value > 0)
		{
			number = num.Value;
			return true;
		}
		number = 0;
		return false;
	}
}
