namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class ConstCalculator : INumericBadgeCalculator
{
	private readonly int _value;

	public ConstCalculator(int value)
	{
		_value = value;
	}

	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		return true;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		number = _value;
		modifier = null;
		return true;
	}
}
