using Appboy;
using UnityEngine;
using Wizards.Mtga.Platforms;

namespace Wizards.Mtga.Notifications;

public class BrazePushNotificationAccountBinding : IPushNotificationAccountBinding
{
	private const string BrazeSubscriptionId = "8d507bb7-0ef8-4d3a-bdae-74b1c960dc2f";

	public void BindAccount(AccountInformation accountInformation)
	{
		if (PlatformUtils.IsHandheld() && !Application.isEditor)
		{
			string externalID = accountInformation.ExternalID;
			string email = accountInformation.Email;
			string displayName = accountInformation.DisplayName;
			AppboyBinding.ChangeUser(externalID);
			AppboyBinding.SetUserEmail(email);
			AppboyBinding.SetUserFirstName(displayName);
			AppboyBinding.AddToSubscriptionGroup("8d507bb7-0ef8-4d3a-bdae-74b1c960dc2f");
		}
	}

	public void UnbindAccount(AccountInformation accountInformation)
	{
	}
}
