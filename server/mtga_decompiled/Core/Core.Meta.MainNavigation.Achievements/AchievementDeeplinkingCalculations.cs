using System;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementDeeplinkingCalculations
{
	public Func<bool> CheckIfAnimationDone;

	public Func<float> GetTotalYdistance;

	public float YdistanceOfTargetCard;

	public Func<float> GetYDistanceOfTargetCardInImmediateParent;
}
