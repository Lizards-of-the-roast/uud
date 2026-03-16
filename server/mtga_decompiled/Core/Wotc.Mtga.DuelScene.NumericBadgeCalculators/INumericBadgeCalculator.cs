namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public interface INumericBadgeCalculator
{
	bool HasNumber(NumericBadgeCalculatorInput input);

	bool GetNumber(NumericBadgeCalculatorInput input, out int number, out string modifier);
}
