namespace Core.Meta.MainNavigation.Achievements;

public class AchievementsScreenWrapperCompassGuide : WrapperCompassGuide
{
	private readonly IClientAchievement _achievement;

	public IClientAchievement Achievement => _achievement;

	public AchievementsScreenWrapperCompassGuide(IClientAchievement achievement)
	{
		_achievement = achievement;
	}
}
