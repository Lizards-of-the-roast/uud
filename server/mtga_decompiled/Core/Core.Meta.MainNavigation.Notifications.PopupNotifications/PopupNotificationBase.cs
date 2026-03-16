using System.Collections;

namespace Core.Meta.MainNavigation.Notifications.PopupNotifications;

public abstract class PopupNotificationBase
{
	public abstract PopupNotificationType Type { get; }

	public abstract IEnumerator ShowCoroutine();
}
