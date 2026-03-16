using Assets.Core.Meta.Utilities;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.Login;

public class HelpPanel : Panel
{
	public void OnButton_Status()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/StatusPage"));
	}

	public void OnButton_Support()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, base.gameObject);
		UrlOpener.OpenURL(Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/CustomerSupport"));
	}

	public void OnButton_ForgotCredentials()
	{
		_loginScene.LoadPanel(PanelType.ForgotCredentials);
	}
}
