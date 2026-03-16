namespace Wotc.Mtga.DuelScene.BadgeActivationCalculators;

public interface IBadgeActivationCalculator
{
	bool GetActive(BadgeActivationCalculatorInput input);
}
