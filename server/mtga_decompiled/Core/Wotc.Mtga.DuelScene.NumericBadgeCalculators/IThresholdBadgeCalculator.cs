namespace Wotc.Mtga.DuelScene.NumericBadgeCalculators;

public interface IThresholdBadgeCalculator
{
	ThresholdBadgeMode BadgeMode { get; }

	bool HasThreshold(NumericBadgeCalculatorInput input);

	bool GetThreshold(NumericBadgeCalculatorInput input, out int threshold);
}
