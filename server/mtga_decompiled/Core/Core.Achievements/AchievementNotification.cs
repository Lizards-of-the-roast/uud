using Core.Meta.MainNavigation.Achievements;

namespace Core.Achievements;

public struct AchievementNotification
{
	private AchievementNotificationType _notificationType;

	private IClientAchievement _achievement;

	public AchievementNotificationType NotificationType => _notificationType;

	public IClientAchievement Achievement => _achievement;

	public AchievementNotification(AchievementNotificationType notificationType, IClientAchievement achievement)
	{
		_notificationType = notificationType;
		_achievement = achievement;
	}
}
