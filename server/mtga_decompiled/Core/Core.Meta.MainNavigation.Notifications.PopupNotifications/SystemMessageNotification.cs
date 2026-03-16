using System.Collections;
using UnityEngine;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.Notifications.PopupNotifications;

public class SystemMessageNotification : PopupNotificationBase
{
	public string TitleLocKey { get; set; }

	public string MessageLocKey { get; set; }

	public override PopupNotificationType Type => PopupNotificationType.SystemMessage;

	public override IEnumerator ShowCoroutine()
	{
		bool popupDismissed = false;
		SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText(TitleLocKey), Languages.ActiveLocProvider.GetLocalizedText(MessageLocKey), showCancel: false, delegate
		{
			popupDismissed = true;
		});
		yield return new WaitUntil(() => popupDismissed);
	}
}
