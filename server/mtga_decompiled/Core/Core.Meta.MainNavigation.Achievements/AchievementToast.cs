using Core.Achievements;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Achievements;

public class AchievementToast : MonoBehaviour
{
	[SerializeField]
	private Localize headerLocalize;

	[SerializeField]
	private TMP_Text headerText;

	[SerializeField]
	private Localize achievementLocalize;

	[SerializeField]
	private Color completedColor;

	[SerializeField]
	private Color inProgressColor;

	public void SetToastData(AchievementNotification toastAchievement)
	{
		if (toastAchievement.Achievement != null)
		{
			headerLocalize.SetText(LocKeyForStatus(toastAchievement.NotificationType));
			headerText.color = ColorForStatus(toastAchievement.NotificationType);
			achievementLocalize.SetText(toastAchievement.Achievement.TitleLocalizationKey);
		}
	}

	private static string LocKeyForStatus(AchievementNotificationType notificationType)
	{
		if (notificationType != AchievementNotificationType.Completed)
		{
			return "PlayBlade/Filters/Default/InProgress";
		}
		return "Achievements/Core/ToastLoc";
	}

	private Color ColorForStatus(AchievementNotificationType notificationType)
	{
		if (notificationType != AchievementNotificationType.Completed)
		{
			return inProgressColor;
		}
		return completedColor;
	}
}
