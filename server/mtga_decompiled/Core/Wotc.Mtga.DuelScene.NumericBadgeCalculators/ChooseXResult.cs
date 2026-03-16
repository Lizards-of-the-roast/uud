namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public class ChooseXResult : INumericBadgeCalculator
{
	public bool HasNumber(NumericBadgeCalculatorInput input)
	{
		return input.CardData?.Instance?.ChooseXResult.HasValue == true;
	}

	public bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier)
	{
		uint? num = input.CardData?.Instance?.ChooseXResult;
		modifier = null;
		if (num.HasValue)
		{
			number = (int)num.Value;
			return true;
		}
		number = 0;
		return false;
	}
}
