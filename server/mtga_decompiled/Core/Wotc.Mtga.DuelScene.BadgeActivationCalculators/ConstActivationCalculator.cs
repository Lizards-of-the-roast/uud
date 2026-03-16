namespace Wotc.Mtga.DuelScene.BadgeActivationCalculators;

public class ConstActivationCalculator : IBadgeActivationCalculator
{
	public bool GetActive(BadgeActivationCalculatorInput input)
	{
		return true;
	}
}
