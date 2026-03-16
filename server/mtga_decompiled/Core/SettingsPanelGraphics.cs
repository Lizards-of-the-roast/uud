using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Quality;

public class SettingsPanelGraphics : SettingsMenuPanel
{
	private const float RATIO_LOWER_LIMIT = 1.59f;

	private const float RATIO_UPPER_LIMIT = 1.81f;

	[SerializeField]
	private TMP_Dropdown _languageDropDown;

	[SerializeField]
	private TMP_Dropdown _qualtityLevelDropdown;

	[SerializeField]
	private CanvasGroup _qualitySettingGroup;

	[SerializeField]
	private SettingsPanelGraphicsControl _qualitySettingPrefab;

	[SerializeField]
	private float _qualitySettingDisabledAlpha = 0.5f;

	[Space(5f)]
	[SerializeField]
	private TMP_Dropdown _resolutionDropdown;

	[SerializeField]
	private Toggle _fullScreentoggle;

	[SerializeField]
	private CustomButton _backButton;

	[Space(5f)]
	[SerializeField]
	private UniversalRenderPipelineAsset _customPipelineAsset;

	private readonly Dictionary<QualitySettingModifier, SettingsPanelGraphicsControl> _qualitySettingsControls = new Dictionary<QualitySettingModifier, SettingsPanelGraphicsControl>();

	private readonly List<Resolution> _resolutionOptions = new List<Resolution>();

	private bool _shouldUpdate;

	private Dictionary<string, string> Languages = new Dictionary<string, string>();

	private volatile bool IsRunningDisplayResolutionUpdate;

	public static bool IsScreenAllowableRatio => IsAllowableRatio(1.59f, 1.81f, Screen.width, Screen.height);

	public static event Action OnWindowChangedEvent;

	public static bool IsAllowableRatio(float lowerLimit, float upperLimit, float actualWidth, float actualHeight)
	{
		float num = actualWidth / actualHeight;
		if (num >= lowerLimit)
		{
			return num <= upperLimit;
		}
		return false;
	}

	public static void ForceWindowed()
	{
		List<Resolution> validWindowResolutions = QualitySettingsHelpers.GetValidWindowResolutions();
		int num = validWindowResolutions.FindIndex((Resolution x) => x.width == Screen.width && x.height == Screen.height);
		Resolution resolution = ((num < 0) ? validWindowResolutions[0] : validWindowResolutions[num]);
		Screen.SetResolution(resolution.width, resolution.height, fullscreen: false);
		SettingsPanelGraphics.OnWindowChangedEvent?.Invoke();
	}

	public static void ForceOptionsUpdate()
	{
		SettingsPanelGraphics.OnWindowChangedEvent?.Invoke();
	}

	public override void Init(SettingsMenu settingsMenu)
	{
		base.Init(settingsMenu);
		ResetQualityLevelDropdown();
		foreach (QualitySettingModifier qualityModifier in QualitySettingsUtil.Instance.QualityModifiers)
		{
			SettingsPanelGraphicsControl settingsPanelGraphicsControl = UnityEngine.Object.Instantiate(_qualitySettingPrefab, _qualitySettingGroup.transform);
			settingsPanelGraphicsControl.gameObject.SetActive(value: true);
			settingsPanelGraphicsControl.name = $"Control - Setting: {qualityModifier.FriendlyName}";
			_qualitySettingsControls.Add(qualityModifier, settingsPanelGraphicsControl);
		}
		_qualitySettingPrefab.gameObject.SetActive(value: false);
		_resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);
		_fullScreentoggle.onValueChanged.AddListener(OnFullScreenToggleChanged);
		_shouldUpdate = true;
		settingsMenu.CloseRequestedHandlers += OnSettingsMenuClosed;
		ResetLanguageDropdown();
	}

	private void OnSettingsMenuClosed()
	{
		QualitySettingsUtil.Instance.SaveCustomSettings();
	}

	public override void ShowPanel()
	{
		_shouldUpdate = true;
		ResetLanguageDropdown();
	}

	public override void HidePanel()
	{
		_shouldUpdate = false;
	}

	private void Awake()
	{
		OnWindowChangedEvent += ShowPanel;
		QualitySettingModifier.OnSettingChanged += QualitySettingModifier_OnSettingChanged;
		_fullScreentoggle.isOn = Screen.fullScreen;
		_backButton.OnClick.AddListener(BackButton_OnClick);
	}

	private void Start()
	{
		ResetLanguageDropdown();
	}

	private void OnDestroy()
	{
		foreach (SettingsPanelGraphicsControl value in _qualitySettingsControls.Values)
		{
			UnityEngine.Object.Destroy(value.gameObject);
		}
		_qualitySettingsControls.Clear();
		OnWindowChangedEvent -= ShowPanel;
		QualitySettingModifier.OnSettingChanged -= QualitySettingModifier_OnSettingChanged;
		_settingsMenu.CloseRequestedHandlers -= OnSettingsMenuClosed;
		_resolutionDropdown.onValueChanged.RemoveAllListeners();
		_fullScreentoggle.onValueChanged.RemoveAllListeners();
		_backButton.OnClick.RemoveAllListeners();
	}

	private void Update()
	{
		if (_settingsMenu.IsOpen && _shouldUpdate)
		{
			UpdateQualitySettingWindow();
			_shouldUpdate = false;
		}
	}

	private void UpdateQualitySettingWindow()
	{
		bool isCustomTier = QualitySettingsUtil.Instance.IsCustomTier;
		QualitySettingsUtil.Instance.ApplySettings();
		foreach (KeyValuePair<QualitySettingModifier, SettingsPanelGraphicsControl> qualitySettingsControl in _qualitySettingsControls)
		{
			string key = "MainNav/Settings/Graphics/QualitySetting_" + qualitySettingsControl.Key.FriendlyName.Replace(" ", string.Empty);
			string localizedText = Wotc.Mtga.Loc.Languages.ActiveLocProvider.GetLocalizedText(key);
			qualitySettingsControl.Value.HeaderLabel.text = localizedText;
			string key2 = "MainNav/Settings/Graphics/QualityValue_" + qualitySettingsControl.Key.CurrentSettingValueName().Replace(" ", string.Empty);
			string localizedText2 = Wotc.Mtga.Loc.Languages.ActiveLocProvider.GetLocalizedText(key2);
			qualitySettingsControl.Value.ValueLabel.text = localizedText2;
			qualitySettingsControl.Value.DecrementButton.onClick.RemoveAllListeners();
			qualitySettingsControl.Value.IncrementButton.onClick.RemoveAllListeners();
			qualitySettingsControl.Value.DecrementButton.gameObject.UpdateActive(isCustomTier);
			qualitySettingsControl.Value.IncrementButton.gameObject.UpdateActive(isCustomTier);
			if (isCustomTier)
			{
				qualitySettingsControl.Value.DecrementButton.onClick.AddListener(qualitySettingsControl.Key.Decrement);
				qualitySettingsControl.Value.IncrementButton.onClick.AddListener(qualitySettingsControl.Key.Increment);
				qualitySettingsControl.Value.DecrementButton.interactable = true;
				qualitySettingsControl.Value.IncrementButton.interactable = true;
			}
		}
		if (isCustomTier)
		{
			_qualitySettingGroup.alpha = 1f;
			_qualitySettingGroup.interactable = true;
		}
		else
		{
			_qualitySettingGroup.alpha = _qualitySettingDisabledAlpha;
			_qualitySettingGroup.interactable = false;
		}
		if (!IsRunningDisplayResolutionUpdate)
		{
			StartCoroutine(coDelayResolutionUpdate());
		}
	}

	private void QualityLevelDropdown_OnValueChanged(int value)
	{
		QualitySettingsUtil.Instance.GlobalQualityLevel = value;
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		_shouldUpdate = true;
	}

	private void QualitySettingModifier_OnSettingChanged(QualitySettingModifier obj)
	{
		_shouldUpdate = true;
	}

	private void OnResolutionDropdownChanged(int newValue)
	{
		Resolution resolution = _resolutionOptions[newValue];
		if (resolution.width != Screen.width || resolution.height != Screen.height)
		{
			Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
			if (!IsRunningDisplayResolutionUpdate)
			{
				StartCoroutine(coDelayResolutionUpdate());
			}
		}
	}

	private void OnFullScreenToggleChanged(bool isFullscreen)
	{
		if (isFullscreen)
		{
			QualitySettingsHelpers.lastWindowedScreenResolution = Screen.currentResolution;
		}
		Resolution resolution = (isFullscreen ? QualitySettingsHelpers.GetValidFullscreenResolutions() : QualitySettingsHelpers.GetValidWindowResolutions())[0];
		Screen.SetResolution(resolution.width, resolution.height, isFullscreen);
		if (!IsRunningDisplayResolutionUpdate)
		{
			StartCoroutine(coDelayResolutionUpdate());
		}
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
	}

	private IEnumerator coDelayResolutionUpdate()
	{
		if (!IsRunningDisplayResolutionUpdate)
		{
			IsRunningDisplayResolutionUpdate = true;
			yield return null;
			yield return null;
			UpdateResolutionDropdownOptions();
			IsRunningDisplayResolutionUpdate = false;
		}
	}

	private void UpdateResolutionDropdownOptions()
	{
		_resolutionOptions.Clear();
		_resolutionOptions.AddRange(Screen.fullScreen ? QualitySettingsHelpers.GetValidFullscreenResolutions() : QualitySettingsHelpers.GetValidWindowResolutions());
		List<string> options = _resolutionOptions.ConvertAll((Resolution x) => Wotc.Mtga.Loc.Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/Graphics/ScreenResolution_Format", ("width", x.width.ToString()), ("height", x.height.ToString())));
		_resolutionDropdown.ClearOptions();
		_resolutionDropdown.AddOptions(options);
		int num = _resolutionOptions.FindLastIndex((Resolution x) => x.width == Screen.width && x.height == Screen.height);
		_resolutionDropdown.value = ((num >= 0) ? num : 0);
	}

	private void BackButton_OnClick()
	{
		_settingsMenu.GoToMainMenu();
	}

	private void ResetQualityLevelDropdown()
	{
		List<TMP_Dropdown.OptionData> options = QualitySettingsUtil.Instance.AvailableTierNames.Select(delegate(string s)
		{
			string key = $"MainNav/Settings/Graphics/QualityLevel_{s}";
			return new TMP_Dropdown.OptionData(Wotc.Mtga.Loc.Languages.ActiveLocProvider.GetLocalizedText(key));
		}).ToList();
		_qualtityLevelDropdown.onValueChanged.RemoveListener(QualityLevelDropdown_OnValueChanged);
		_qualtityLevelDropdown.options = options;
		_qualtityLevelDropdown.value = QualitySettingsUtil.Instance.GlobalQualityLevel;
		_qualtityLevelDropdown.onValueChanged.AddListener(QualityLevelDropdown_OnValueChanged);
	}

	private void OnLanguageDropdown(int val)
	{
		string language = null;
		if (Languages.TryGetValue(_languageDropDown.options[val].text, out language))
		{
			Wotc.Mtga.Loc.Languages.LanguageChangedSignal.Listeners += onLangChange;
			MDNPlayerPrefs.PLAYERPREFS_ClientLanguage = language;
			Wotc.Mtga.Loc.Languages.CurrentLanguage = language;
			Wotc.Mtga.Loc.Languages.LanguageChangedSignal.Listeners -= onLangChange;
		}
		void onLangChange()
		{
			MDNPlayerPrefs.PLAYERPREFS_ClientLanguage = language;
			if (!_settingsMenu.IsOpen)
			{
				UpdateQualitySettingWindow();
				ResetQualityLevelDropdown();
			}
			ResetLanguageDropdown();
		}
	}

	private void ResetLanguageDropdown()
	{
		_languageDropDown.onValueChanged.RemoveListener(OnLanguageDropdown);
		Languages.Clear();
		string[] externalLanguages = Wotc.Mtga.Loc.Languages.ExternalLanguages;
		foreach (string text in externalLanguages)
		{
			string nativeLanguageValue = GetNativeLanguageValue(text);
			Languages.Add(nativeLanguageValue, text);
		}
		List<TMP_Dropdown.OptionData> options = Languages.Select((KeyValuePair<string, string> s) => new TMP_Dropdown.OptionData(s.Key)).ToList();
		_languageDropDown.options = options;
		_languageDropDown.value = 0;
		int dropDownIndexOfLanguage = GetDropDownIndexOfLanguage(MDNPlayerPrefs.PLAYERPREFS_ClientLanguage);
		if (dropDownIndexOfLanguage != -1)
		{
			_languageDropDown.value = dropDownIndexOfLanguage;
		}
		_languageDropDown.onValueChanged.AddListener(OnLanguageDropdown);
	}

	private static string GetNativeLanguageValue(string lang)
	{
		string text = "";
		return Wotc.Mtga.Loc.Languages.MTGAtoI2LangCode[lang] switch
		{
			"en" => "English", 
			"de" => "Deutsch", 
			"es" => "Español", 
			"fr" => "Français", 
			"it" => "Italiano", 
			"ja" => "日本語", 
			"ko" => "한국어", 
			"pt-BR" => "Português\u00a0brasileiro", 
			"ru" => "Русский", 
			"zh-CN" => "简体中文", 
			_ => lang + " NEEDS NATIVE TRANSLATION", 
		};
	}

	private int GetDropDownIndexOfLanguage(string language)
	{
		string localizedDisplay = Languages.FirstOrDefault((KeyValuePair<string, string> pair) => pair.Value.Equals(language)).Key;
		int result = -1;
		if (localizedDisplay != null)
		{
			result = _languageDropDown.options.FindIndex((TMP_Dropdown.OptionData x) => x.text == localizedDisplay);
		}
		return result;
	}
}
