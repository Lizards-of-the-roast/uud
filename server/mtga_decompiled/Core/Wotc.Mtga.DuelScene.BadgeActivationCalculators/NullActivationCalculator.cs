namespace Wotc.Mtga.DuelScene.BadgeActivationCalculators;

public class NullActivationCalculator : IBadgeActivationCalculator
{
	public bool GetActive(BadgeActivationCalculatorInput input)
	{
		return false;
	}
}
