using System.Collections;
using System.Collections.Concurrent;
using Core.Meta.MainNavigation.Notifications.PopupNotifications;

namespace Core.Meta.MainNavigation.Notifications;

public class PopupNotificationManager
{
	private readonly ConcurrentQueue<PopupNotificationBase> _popupQueue = new ConcurrentQueue<PopupNotificationBase>();

	public static PopupNotificationManager Create()
	{
		return new PopupNotificationManager();
	}

	public void Enqueue(PopupNotificationBase notification)
	{
		_popupQueue.Enqueue(notification);
	}

	public IEnumerator ShowPopupsCoroutine()
	{
		PopupNotificationBase result;
		while (_popupQueue.TryDequeue(out result))
		{
			yield return result.ShowCoroutine();
		}
	}
}
