using System.Collections.Generic;
using Assets.Core.Meta.Utilities;
using Wotc.Mtga.Loc;

public static class LoginUtils
{
	public static void ShowAgeGateRegistrationFailurePopup()
	{
		SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Minimum_Age_Registration_Unsuccessful_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/Minimum_Age_Registration_Unsuccessful_Description"), new List<SystemMessageManager.SystemMessageButtonData>
		{
			new SystemMessageManager.SystemMessageButtonData
			{
				Text = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/ClientPrompt/ClientPrompt_Button_No")
			},
			new SystemMessageManager.SystemMessageButtonData
			{
				Text = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/ClientPrompt/ClientPrompt_Button_Yes"),
				Callback = delegate
				{
					UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/AccountPreferences"));
				},
				IsExternalLink = true
			}
		});
	}

	public static void ShowUpdateParentalConsentFailurePopup()
	{
		SystemMessageManager.Instance.ShowMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/KID_UpdateUnsuccessfulHeader"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Login/KID_UpdateUnsuccessfulDesc"), new List<SystemMessageManager.SystemMessageButtonData>
		{
			new SystemMessageManager.SystemMessageButtonData
			{
				Text = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/ClientPrompt/ClientPrompt_Button_No")
			},
			new SystemMessageManager.SystemMessageButtonData
			{
				Text = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/ClientPrompt/ClientPrompt_Button_Yes"),
				Callback = delegate
				{
					UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/AccountPreferences"));
				},
				IsExternalLink = true
			}
		});
	}
}
