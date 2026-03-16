using Core.Code.Input;
using MTGA.KeyboardManager;
using UnityEngine;
using Wizards.Mtga.Platforms;

namespace Wotc.Mtga.Login;

public class RegisterOrLoginPanel : Panel
{
	[SerializeField]
	private GameObject registerButton;

	private SettingsMenuHost _settings;

	public void InjectSettings(SettingsMenuHost settings)
	{
		_settings = settings;
	}

	public override void Show()
	{
		_loginScene._accountClient.Reset();
		registerButton.SetActive(_loginScene._accountClient.AllowAccountCreation);
		base.Show();
	}

	public void OnButton_GoToLogin()
	{
		_loginScene.LoadPanel(PanelType.LogIn);
	}

	public void OnButton_GoToRegistration()
	{
		_loginScene.LoadPanel(PanelType.BirthLanguage);
	}

	public void UpdateAllowsRegistration(bool allowRegistration)
	{
		registerButton.SetActive(allowRegistration);
	}

	public override bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		return false;
	}

	public override void OnBack(ActionContext context)
	{
		if (PlatformUtils.IsHandheld() && !Application.isEditor)
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept_small, base.gameObject);
			SceneLoader.ApplicationQuit();
		}
		else
		{
			_settings.Open();
		}
	}
}
