namespace Wizards.Mtga.Notifications;

public interface IPushNotificationAccountBinding
{
	void BindAccount(AccountInformation accountInformation);

	void UnbindAccount(AccountInformation accountInformation);
}
