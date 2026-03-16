namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class NullNumericBadgeCalculator : INumericBadgeCalculator
{
	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		return false;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		number = 0;
		modifier = null;
		return false;
	}
}
