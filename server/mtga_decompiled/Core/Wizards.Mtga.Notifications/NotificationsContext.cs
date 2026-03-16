namespace Wizards.Mtga.Notifications;

public class NotificationsContext
{
	public void InitializePushNotifications(IAccountClient accountClient)
	{
		BrazePushNotificationAccountBinding binding = new BrazePushNotificationAccountBinding();
		new PushNotificationBinder(accountClient, binding);
	}
}
