using UnityEngine;

public class SettingsPanelDevTools : SettingsMenuPanel
{
	[SerializeField]
	private CustomButton _backButton;

	private void Awake()
	{
		_backButton.OnClick.AddListener(BackButton_OnClick);
	}

	public override void ShowPanel()
	{
	}

	public override void HidePanel()
	{
	}

	private void BackButton_OnClick()
	{
		_settingsMenu.GoToMainMenu();
	}

	private void OnDestroy()
	{
		_backButton.OnClick.RemoveAllListeners();
	}
}
