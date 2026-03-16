using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga;
using Wotc.Mtga.DuelScene.Companions;
using Wotc.Mtga.DuelScene.Emotes;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

public class SettingsPanelGameplay : SettingsMenuPanel
{
	[SerializeField]
	private Toggle _autoPayToggle;

	[SerializeField]
	private Toggle _disableEmotesToggle;

	[SerializeField]
	private Toggle _evergreenKeywordsToggle;

	[SerializeField]
	private Toggle _autoTriggersToggle;

	[SerializeField]
	private Toggle _autoReplacementsToggle;

	[SerializeField]
	private Toggle _phaseLadderToggle;

	[SerializeField]
	private Toggle _autoApplyCardStylesToggle;

	[SerializeField]
	private Toggle _fixedRulesTextSizeToggle;

	[SerializeField]
	private CustomButton _backButton;

	[SerializeField]
	private GameObject _autoPayDisabled;

	[SerializeField]
	private GameObject _phaseLadderDisabled;

	[SerializeField]
	private Toggle _hideAltArtStylesToggle;

	[SerializeField]
	private Toggle _showGameplayWarningsToggle;

	[SerializeField]
	private GameObject _hideAltArtDisabled;

	[SerializeField]
	private GameObject _autoReplacementsDisabled;

	[Space(3f)]
	[SerializeField]
	private Localize _floatAllKeybindingText;

	[SerializeField]
	private Localize _quickTapKeybindingText;

	[SerializeField]
	private Localize _ShowCollectionInDraftKeybindingText;

	[SerializeField]
	private bool _DisablePhaseLadderToggle;

	private SettingsMessage _settingsWhenSent;

	private GameManager _gameManager;

	private void Awake()
	{
		_autoPayToggle.onValueChanged.AddListener(OnAutoPayToggled);
		_disableEmotesToggle.onValueChanged.AddListener(OnDisableEmotesToggled);
		_evergreenKeywordsToggle.onValueChanged.AddListener(OnEvergreenKeywordsToggled);
		_autoTriggersToggle.onValueChanged.AddListener(OnAutoTriggersToggled);
		_autoReplacementsToggle.onValueChanged.AddListener(OnAutoReplacementsToggled);
		_phaseLadderToggle.onValueChanged.AddListener(OnPhaseLadderToggled);
		_autoApplyCardStylesToggle.onValueChanged.AddListener(OnAutoApplyCardStylesToggled);
		_fixedRulesTextSizeToggle.onValueChanged.AddListener(OnFixedRulesTextSizeToggled);
		_backButton.OnClick.AddListener(BackButton_OnClick);
		_hideAltArtStylesToggle.onValueChanged.AddListener(OnHideAltArtStylesToggled);
		_showGameplayWarningsToggle.onValueChanged.AddListener(OnEnableGameplayWarningsToggled);
		_floatAllKeybindingText.SetText("DuelScene/SettingsMenu/Gameplay/DoubleTap", new Dictionary<string, string> { { "key", "Q" } });
		_quickTapKeybindingText.SetText("DuelScene/SettingsMenu/Gameplay/PressAndHold", new Dictionary<string, string> { { "key", "Q" } });
		_ShowCollectionInDraftKeybindingText.SetText("DuelScene/SettingsMenu/Gameplay/PressAndHold", new Dictionary<string, string> { { "key", "Alt" } });
	}

	public override void ShowPanel()
	{
		_gameManager = Object.FindObjectOfType<GameManager>();
		if (!_gameManager)
		{
			_autoPayDisabled.SetActive(value: true);
			_autoPayToggle.gameObject.SetActive(value: false);
			_autoReplacementsDisabled.SetActive(value: true);
			_autoReplacementsToggle.gameObject.SetActive(value: false);
			_hideAltArtDisabled.SetActive(value: false);
			_hideAltArtStylesToggle.gameObject.SetActive(value: true);
			_hideAltArtStylesToggle.isOn = MDNPlayerPrefs.HideAltArtStyles;
			_hideAltArtStylesToggle.interactable = true;
		}
		else
		{
			_autoPayDisabled.SetActive(value: false);
			_autoPayToggle.gameObject.SetActive(value: true);
			_autoReplacementsDisabled.SetActive(value: false);
			_autoReplacementsToggle.gameObject.SetActive(value: true);
			_hideAltArtDisabled.SetActive(value: true);
			_hideAltArtStylesToggle.gameObject.SetActive(value: false);
			_autoPayToggle.isOn = _gameManager.AutoRespManager.AutoPayManaEnabled;
			_autoPayToggle.interactable = true;
		}
		_disableEmotesToggle.isOn = MDNPlayerPrefs.DisableEmotes;
		_disableEmotesToggle.interactable = true;
		_evergreenKeywordsToggle.isOn = MDNPlayerPrefs.ShowEvergreenKeywordReminders;
		_evergreenKeywordsToggle.interactable = true;
		_autoTriggersToggle.isOn = MDNPlayerPrefs.AutoOrderTriggers;
		_autoTriggersToggle.interactable = true;
		_autoReplacementsToggle.isOn = MDNPlayerPrefs.AutoChooseReplacementEffects;
		_autoReplacementsToggle.interactable = true;
		if (_DisablePhaseLadderToggle)
		{
			_phaseLadderDisabled.SetActive(value: true);
			_phaseLadderToggle.interactable = false;
			_phaseLadderToggle.gameObject.SetActive(value: false);
		}
		else
		{
			_phaseLadderDisabled.SetActive(value: false);
			_phaseLadderToggle.isOn = MDNPlayerPrefs.ShowPhaseLadder;
			_phaseLadderToggle.interactable = true;
			_phaseLadderToggle.gameObject.SetActive(value: true);
		}
		_autoApplyCardStylesToggle.isOn = MDNPlayerPrefs.AutoApplyCardStyles;
		_autoApplyCardStylesToggle.interactable = true;
		_showGameplayWarningsToggle.isOn = MDNPlayerPrefs.GameplayWarningsEnabled;
		_showGameplayWarningsToggle.interactable = true;
		_fixedRulesTextSizeToggle.isOn = MDNPlayerPrefs.FixedRulesTextSize;
		_fixedRulesTextSizeToggle.interactable = true;
	}

	public override void HidePanel()
	{
		_settingsWhenSent = null;
		_gameManager = null;
	}

	private void LateUpdate()
	{
		if (_settingsWhenSent != null && (bool)_gameManager && _gameManager.CurrentSettings != _settingsWhenSent)
		{
			_autoPayToggle.isOn = _gameManager.AutoRespManager.AutoPayManaEnabled;
			_autoPayToggle.interactable = true;
			_autoReplacementsToggle.isOn = _gameManager.AutoRespManager.AutoSelectReplacementEffects;
			_autoReplacementsToggle.interactable = true;
			_settingsWhenSent = null;
		}
	}

	private void OnAutoPayToggled(bool value)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		if ((bool)_gameManager)
		{
			_autoPayToggle.interactable = false;
			_settingsWhenSent = _gameManager.CurrentSettings;
			_gameManager.AutoRespManager.SetManaAutoPayment(value);
		}
	}

	private void OnDisableEmotesToggled(bool value)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		MDNPlayerPrefs.DisableEmotes = value;
		if (!(_gameManager == null))
		{
			IContext context = _gameManager.Context;
			if ((context.Get<ICompanionViewProvider>() ?? NullCompanionViewProvider.Default).TryGetCompanionByPlayerType(GREPlayerNum.Opponent, out var view))
			{
				view.OnGlobalMuteChanged(value);
			}
			(context.Get<IEmoteManager>() ?? NullEmoteManager.Default).MuteEmotes(value);
		}
	}

	private void OnEvergreenKeywordsToggled(bool value)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		MDNPlayerPrefs.ShowEvergreenKeywordReminders = value;
	}

	private void OnAutoTriggersToggled(bool value)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		MDNPlayerPrefs.AutoOrderTriggers = value;
	}

	private void OnAutoReplacementsToggled(bool value)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		if ((bool)_gameManager)
		{
			_autoReplacementsToggle.interactable = false;
			_settingsWhenSent = _gameManager.CurrentSettings;
			_gameManager.AutoRespManager.SetAutoSelectReplacementSetting(value);
		}
	}

	private void OnPhaseLadderToggled(bool value)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		MDNPlayerPrefs.ShowPhaseLadder = value;
		MDNPlayerPrefs.SeenPhaseLadderHint = true;
		if ((bool)_gameManager)
		{
			_gameManager.UIManager.PhaseLadder.OnEnable();
		}
	}

	private void OnAutoApplyCardStylesToggled(bool value)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		MDNPlayerPrefs.AutoApplyCardStyles = value;
	}

	private void OnFixedRulesTextSizeToggled(bool value)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		MDNPlayerPrefs.FixedRulesTextSize = value;
		Languages.TriggerLocalizationRefresh();
	}

	private void OnHideAltArtStylesToggled(bool value)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		MDNPlayerPrefs.HideAltArtStyles = value;
	}

	private void OnEnableGameplayWarningsToggled(bool value)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		MDNPlayerPrefs.GameplayWarningsEnabled = value;
	}

	private void BackButton_OnClick()
	{
		_settingsMenu.GoToMainMenu();
	}

	private void OnDestroy()
	{
		_gameManager = null;
		_autoPayToggle.onValueChanged.RemoveAllListeners();
		_disableEmotesToggle.onValueChanged.RemoveAllListeners();
		_evergreenKeywordsToggle.onValueChanged.RemoveAllListeners();
		_autoTriggersToggle.onValueChanged.RemoveAllListeners();
		_autoReplacementsToggle.onValueChanged.RemoveAllListeners();
		_phaseLadderToggle.onValueChanged.RemoveAllListeners();
		_autoApplyCardStylesToggle.onValueChanged.RemoveAllListeners();
		_fixedRulesTextSizeToggle.onValueChanged.RemoveAllListeners();
		_backButton.OnClick.RemoveAllListeners();
		_hideAltArtStylesToggle.onValueChanged.RemoveAllListeners();
		_showGameplayWarningsToggle.onValueChanged.RemoveAllListeners();
	}
}
