using Core.BI;
using Core.Shared.Code.Connection;
using UnityEngine;
using Wizards.Mtga;
using Wotc.Mtga.Loc;

namespace Core.Meta.MainNavigation.SettingsMenu;

public class ValidateBundlesButton : MonoBehaviour
{
	public void Event_OnClick()
	{
		SystemMessageManager.ShowSystemMessage(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/ReportABug/ValidateBundles_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/ReportABug/ValidateBundles_Body"), showCancel: true, RunValidation);
	}

	private void RunValidation()
	{
		BIEventType.UserInitiatedBundleValidation.SendWithDefaults();
		MDNPlayerPrefs.HashAllFilesOnStartup = true;
		Pantry.Get<FrontDoorConnectionManager>().RestartGame("Bundle Validation Requested");
	}
}
