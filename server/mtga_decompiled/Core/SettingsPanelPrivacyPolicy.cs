using Assets.Core.Meta.Utilities;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Loc;

public class SettingsPanelPrivacyPolicy : SettingsMenuPanel
{
	[SerializeField]
	private CustomButton _backButton;

	[Header("Links")]
	[SerializeField]
	private Button _fullPolicyTop;

	[SerializeField]
	private Button _esrbImageLink;

	[SerializeField]
	private Button _esrbLink;

	[SerializeField]
	private Button _aboutAdds;

	private void Awake()
	{
		_backButton.OnClick.AddListener(delegate
		{
			_settingsMenu.GoToMainMenu();
		});
		_fullPolicyTop.onClick.AddListener(delegate
		{
			ClickedLink(Languages.ActiveLocProvider.GetLocalizedText("MainNav/WebLink/PrivacyPolicy"));
		});
		_esrbLink.onClick.AddListener(delegate
		{
			ClickedLink(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/PrivacyPolicy/ESRBLink"));
		});
		_esrbImageLink.onClick.AddListener(delegate
		{
			ClickedLink(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/PrivacyPolicy/ESRBLink"));
		});
		_aboutAdds.onClick.AddListener(delegate
		{
			ClickedLink(Languages.ActiveLocProvider.GetLocalizedText("Duelscene/SettingsMenu/PrivacyPolicy/AboutAds_Link"));
		});
	}

	private void ClickedLink(string url)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		UrlOpener.OpenURL(url);
	}

	private void OnDestroy()
	{
		_backButton.OnClick.RemoveAllListeners();
		_fullPolicyTop.onClick.RemoveAllListeners();
		_esrbLink.onClick.RemoveAllListeners();
		_esrbImageLink.onClick.RemoveAllListeners();
		_aboutAdds.onClick.RemoveAllListeners();
	}
}
