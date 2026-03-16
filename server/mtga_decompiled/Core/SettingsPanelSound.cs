using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsPanelSound : SettingsMenuPanel
{
	[SerializeField]
	private Slider _masterSlider;

	[SerializeField]
	private TMP_Text _masterLabel;

	[SerializeField]
	private Slider _musicSlider;

	[SerializeField]
	private TMP_Text _musicLabel;

	[SerializeField]
	private Slider _ambientSlider;

	[SerializeField]
	private TMP_Text _ambientLabel;

	[SerializeField]
	private Slider _sfxSlider;

	[SerializeField]
	private TMP_Text _sfxLabel;

	[SerializeField]
	private Slider _VOSlider;

	[SerializeField]
	private TMP_Text _VOLabel;

	[SerializeField]
	private Toggle _backgroundAudioToggle;

	[SerializeField]
	private CustomButton _backButton;

	private void Awake()
	{
		_masterSlider.onValueChanged.AddListener(OnMasterUpdated);
		_musicSlider.onValueChanged.AddListener(OnMusicUpdated);
		_ambientSlider.onValueChanged.AddListener(OnAmbianceUpdated);
		_sfxSlider.onValueChanged.AddListener(OnSFXUpdated);
		_VOSlider.onValueChanged.AddListener(OnVOUpdated);
		_backgroundAudioToggle.onValueChanged.AddListener(OnBackgroundToggle);
		_backButton.OnClick.AddListener(BackButton_OnClick);
	}

	public override void ShowPanel()
	{
		_masterSlider.value = AudioManager.GetMasterVolume() * 0.01f;
		_musicSlider.value = AudioManager.GetMusicVolume() * 0.01f;
		_ambientSlider.value = AudioManager.GetAmbienceVolume() * 0.01f;
		_sfxSlider.value = AudioManager.GetSFXVolume() * 0.01f;
		_VOSlider.value = AudioManager.GetVOVolume() * 0.01f;
		_masterLabel.SetText(_masterSlider.value.ToString("P0"));
		_musicLabel.SetText(_musicSlider.value.ToString("P0"));
		_ambientLabel.SetText(_ambientSlider.value.ToString("P0"));
		_sfxLabel.SetText(_sfxSlider.value.ToString("P0"));
		_VOLabel.SetText(_VOSlider.value.ToString("P0"));
		_backgroundAudioToggle.isOn = MDNPlayerPrefs.PLAYERPREFS_KEY_BACKGROUNDAUDIO;
	}

	public override void HidePanel()
	{
	}

	private void OnMasterUpdated(float value)
	{
		_masterLabel.SetText(value.ToString("P0"));
		AudioManager.SetMasterVolume(value * 100f);
	}

	private void OnMusicUpdated(float value)
	{
		_musicLabel.SetText(value.ToString("P0"));
		AudioManager.SetMusicVolume(value * 100f);
	}

	private void OnAmbianceUpdated(float value)
	{
		_ambientLabel.SetText(value.ToString("P0"));
		AudioManager.SetAmbienceVolume(value * 100f);
	}

	private void OnSFXUpdated(float value)
	{
		_sfxLabel.SetText(value.ToString("P0"));
		AudioManager.SetSFXVolume(value * 100f);
	}

	private void OnVOUpdated(float value)
	{
		_VOLabel.SetText(value.ToString("P0"));
		AudioManager.SetVOVolume(value * 100f);
	}

	private void OnBackgroundToggle(bool value)
	{
		MDNPlayerPrefs.PLAYERPREFS_KEY_BACKGROUNDAUDIO = value;
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
	}

	private void BackButton_OnClick()
	{
		_settingsMenu.GoToMainMenu();
	}

	private void OnDestroy()
	{
		_masterSlider.onValueChanged.RemoveAllListeners();
		_musicSlider.onValueChanged.RemoveAllListeners();
		_ambientSlider.onValueChanged.RemoveAllListeners();
		_sfxSlider.onValueChanged.RemoveAllListeners();
		_VOSlider.onValueChanged.RemoveAllListeners();
		_backgroundAudioToggle.onValueChanged.RemoveAllListeners();
		_backButton.OnClick.RemoveAllListeners();
	}
}
