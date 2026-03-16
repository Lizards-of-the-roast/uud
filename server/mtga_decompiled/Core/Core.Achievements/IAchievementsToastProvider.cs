using System;
using System.Collections.Generic;
using Core.Meta.MainNavigation.Achievements;
using Wizards.Unification.Models.Graph;

namespace Core.Achievements;

public interface IAchievementsToastProvider
{
	event Action ToastReceived;

	bool HasAchievementNotificationInQueue(params AchievementNotificationType[] achievementTypesToCount);

	IReadOnlyList<AchievementNotification> GetMultipleNextAchievementNotificationsInQueue(int numberToRetrieve = int.MaxValue, params AchievementNotificationType[] achievementTypesToGet);

	IReadOnlyList<AchievementNotification> GetMultipleNextAchievementNotificationsInQueue(params AchievementNotificationType[] achievementTypesToGet);

	void AddAchievementToast(CampaignGraphDeltas response, IClientAchievement achievement);
}
