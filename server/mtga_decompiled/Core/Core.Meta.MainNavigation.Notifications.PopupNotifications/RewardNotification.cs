using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Meta.MainNavigation.Store;
using UnityEngine;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Notifications.PopupNotifications;

public class RewardNotification : PopupNotificationBase
{
	public override PopupNotificationType Type => PopupNotificationType.Reward;

	public string TitleLocKey { get; set; }

	public string ButtonTextLocKey { get; set; }

	public string SubtitleLocKey { get; set; }

	public List<ClientInventoryUpdateReportItem> Update { get; set; }

	public override IEnumerator ShowCoroutine()
	{
		ContentControllerRewards rewardsContentController = SceneLoader.GetSceneLoader().GetRewardsContentController();
		bool popupDismissed = false;
		rewardsContentController.RegisterRewardWillCloseCallback(delegate
		{
			popupDismissed = true;
		});
		string subtitle = ((SubtitleLocKey != null) ? Languages.ActiveLocProvider.GetLocalizedText(ButtonTextLocKey) : null);
		yield return rewardsContentController.AddAndDisplayRewardsCoroutine(Update, Languages.ActiveLocProvider.GetLocalizedText(TitleLocKey), Languages.ActiveLocProvider.GetLocalizedText(ButtonTextLocKey), subtitle);
		foreach (string item in Update.SelectMany((ClientInventoryUpdateReportItem x) => x.delta.vanityItemsAdded))
		{
			if (item.ToLower().StartsWith("cardbacks.") && !MDNPlayerPrefs.HasSeenSleeveNotify)
			{
				MDNPlayerPrefs.HasSeenSleeveNotify = true;
				MDNPlayerPrefs.FirstTimeSleeveNotify = true;
				break;
			}
		}
		yield return new WaitUntil(() => popupDismissed);
	}
}
