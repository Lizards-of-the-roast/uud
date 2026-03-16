using UnityEngine;

public class SettingsPanelHotkeys : SettingsMenuPanel
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
}
