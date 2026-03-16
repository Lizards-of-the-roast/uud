using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AssetLookupTree;
using AssetLookupTree.Payloads.Wrapper;
using Core.Code.Input;
using Core.Meta.MainNavigation.Challenge;
using Core.Shared.Code;
using MTGA.Social;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class SettingsMenu : MonoBehaviour, IBackActionHandler, IActionBlocker
{
	[Serializable]
	private struct SettingsPanel
	{
		public CustomButton Button;

		public SettingsMenuPanel Panel;

		public bool DebugOnly;

		public bool Hide;
	}

	[SerializeField]
	private GameObject LogoutButton;

	[SerializeField]
	private GameObject ExitGameButton;

	[SerializeField]
	private GameObject ConcedeButton;

	[SerializeField]
	private GameObject SkipTutorialButton;

	[SerializeField]
	private GameObject ExperimentalSkipTutorialButton;

	[SerializeField]
	private GameObject SkipOnboardingButton;

	[SerializeField]
	private GameObject MainMenuPanel;

	[SerializeField]
	private Toggle DetailedLoggingToggle;

	[SerializeField]
	private GameObject SubMenuBackground;

	[SerializeField]
	private GameObject MainMenuBackground;

	[SerializeField]
	private Image _mainBackgroundImage;

	[SerializeField]
	private Image _subBackgroundImage;

	[SerializeField]
	private CanvasGroup _canvasGroup;

	[SerializeField]
	private List<SettingsPanel> _settingsPanels = new List<SettingsPanel>();

	private LoggingConfig _loggingConfig;

	private ISocialManager _socialManager;

	private PVPChallengeController _challengeController;

	private SettingsPanelAccount _accountPanel;

	private SettingsMenuPanel _activePanel;

	private bool _allowMatchConcession;

	public bool IsOpen { get; private set; }

	public bool IsMainPanelActive => _activePanel == null;

	public event Action DestroyedHandlers;

	public event Action EnableDetailedLogsRequestedHandlers;

	public event Action DisableDetailedLogsRequestedHandlers;

	public event Action EnableBlockFriendRequestsRequestedHandlers;

	public event Action DisableBlockFriendRequestsRequestedHandlers;

	public event Action EnableBlockNonFriendChallengesHandlers;

	public event Action DisableBlockNonFriendChallengesHandlers;

	public event Action CloseRequestedHandlers;

	public event Action LogoutRequestedHandlers;

	public event Action ExitApplicationRequestedHandlers;

	public event Action SkipTutorialRequestedHandlers;

	public event Func<Task> SkipOnboardingRequestedHandlers;

	public event Action ConcedeGameRequestedHandlers;

	public event Action ConcedeMatchRequestedHandlers;

	public void Open(bool allowLogout, bool allowExit, bool allowGameConcession, bool allowMatchConcession, bool allowSkipTutorial, bool allowSkipOnboarding, bool allowDebug)
	{
		_canvasGroup.alpha = 1f;
		_canvasGroup.interactable = true;
		_canvasGroup.blocksRaycasts = true;
		base.gameObject.UpdateActive(active: true);
		foreach (SettingsPanel settingsPanel in _settingsPanels)
		{
			bool flag = !settingsPanel.DebugOnly || allowDebug;
			if (settingsPanel.Hide)
			{
				flag = false;
			}
			settingsPanel.Button.gameObject.SetActive(flag);
			bool activeSelf = settingsPanel.Panel.gameObject.activeSelf;
			if (!flag && activeSelf)
			{
				settingsPanel.Panel.HidePanel();
				settingsPanel.Panel.gameObject.SetActive(value: false);
			}
		}
		LogoutButton.SetActive(allowLogout);
		ExitGameButton.SetActive(allowExit);
		_allowMatchConcession = allowMatchConcession;
		ConcedeButton.SetActive(allowGameConcession || allowMatchConcession);
		SkipTutorialButton.SetActive(value: false);
		ExperimentalSkipTutorialButton.SetActive(allowSkipTutorial);
		SkipOnboardingButton.SetActive(allowSkipOnboarding);
		if (PlatformUtils.IsHandheld())
		{
			DetailedLoggingToggle.gameObject.SetActive(allowDebug);
		}
		GoToMainMenu();
		IsOpen = true;
	}

	public bool TrySetBackgroundImages(SettingsBackgroundPayload backgroundPayload)
	{
		if (backgroundPayload != null)
		{
			AssetLoader.AssetTracker<Sprite> assetTracker = new AssetLoader.AssetTracker<Sprite>("SettingsMenuBackgrounds");
			bool num = TrySetBackgroundImage(_mainBackgroundImage, backgroundPayload.MainPanelReference, assetTracker);
			bool flag = TrySetBackgroundImage(_subBackgroundImage, backgroundPayload.SubPanelReference, assetTracker);
			if (num && flag)
			{
				return true;
			}
		}
		return false;
	}

	private bool TrySetBackgroundImage(Image backgroundImage, AltAssetReference<Sprite> backgroundSpriteReference, AssetLoader.AssetTracker<Sprite> assetTracker)
	{
		if (backgroundSpriteReference != null && !string.IsNullOrEmpty(backgroundSpriteReference.RelativePath))
		{
			Sprite sprite = assetTracker.Acquire(backgroundSpriteReference.RelativePath);
			if (sprite != null)
			{
				backgroundImage.sprite = sprite;
				assetTracker.Cleanup();
				return true;
			}
		}
		return false;
	}

	public void Close()
	{
		base.gameObject.UpdateActive(active: false);
		IsOpen = false;
	}

	public void GoToMainMenu()
	{
		GoToPanel(null);
	}

	private void Awake()
	{
		foreach (SettingsPanel panel in _settingsPanels)
		{
			panel.Button.OnClick.AddListener(delegate
			{
				GoToPanel(panel.Panel);
			});
			panel.Button.gameObject.SetActive(value: false);
			panel.Panel.gameObject.SetActive(value: false);
			panel.Panel.Init(this);
		}
		_accountPanel = GetComponentInChildren<SettingsPanelAccount>(includeInactive: true);
		_accountPanel.DetailedLogsToggled += OnDetailedLogsToggled;
		_accountPanel.BlockFriendRequestsToggled += OnBlockFriendRequestsToggled;
		_accountPanel.BlockNonFriendChallengesToggled += OnBlockNonFriendChallengesToggled;
	}

	public void Init(LoggingConfig loggingConfig, ISocialManager socialManager, PVPChallengeController challengeController)
	{
		_loggingConfig = loggingConfig;
		_socialManager = socialManager;
		_challengeController = challengeController;
		_accountPanel.Init(_loggingConfig, _socialManager, _challengeController);
	}

	private void OnDestroy()
	{
		if ((bool)_accountPanel)
		{
			_accountPanel.DetailedLogsToggled -= OnDetailedLogsToggled;
			_accountPanel.BlockFriendRequestsToggled -= OnBlockFriendRequestsToggled;
			_accountPanel.BlockNonFriendChallengesToggled -= OnBlockNonFriendChallengesToggled;
			_accountPanel = null;
		}
		this.EnableDetailedLogsRequestedHandlers = null;
		this.DisableDetailedLogsRequestedHandlers = null;
		this.EnableBlockFriendRequestsRequestedHandlers = null;
		this.DisableBlockFriendRequestsRequestedHandlers = null;
		this.CloseRequestedHandlers = null;
		this.LogoutRequestedHandlers = null;
		this.ExitApplicationRequestedHandlers = null;
		this.SkipTutorialRequestedHandlers = null;
		this.SkipOnboardingRequestedHandlers = null;
		this.ConcedeGameRequestedHandlers = null;
		this.ConcedeMatchRequestedHandlers = null;
		Action action = this.DestroyedHandlers;
		this.DestroyedHandlers = null;
		action?.Invoke();
	}

	private void OnDetailedLogsToggled(bool isOn)
	{
		if (isOn)
		{
			this.EnableDetailedLogsRequestedHandlers?.Invoke();
		}
		else
		{
			this.DisableDetailedLogsRequestedHandlers?.Invoke();
		}
	}

	private void OnBlockFriendRequestsToggled(bool isOn)
	{
		if (isOn)
		{
			this.EnableBlockFriendRequestsRequestedHandlers?.Invoke();
		}
		else
		{
			this.DisableBlockFriendRequestsRequestedHandlers?.Invoke();
		}
	}

	private void OnBlockNonFriendChallengesToggled(bool isOn)
	{
		if (isOn)
		{
			this.EnableBlockNonFriendChallengesHandlers?.Invoke();
		}
		else
		{
			this.DisableBlockNonFriendChallengesHandlers?.Invoke();
		}
	}

	public void OnCloseClicked()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		this.CloseRequestedHandlers?.Invoke();
	}

	public void ExitButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Confirm_ExitGame_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Confirm_ExitGame_Text"), delegate
		{
			this.ExitApplicationRequestedHandlers?.Invoke();
		}, null);
	}

	public void LogOutButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Confirm_LogOut_Title"), Languages.ActiveLocProvider.GetLocalizedText("SystemMessage/System_Confirm_LogOut_Text"), delegate
		{
			this.LogoutRequestedHandlers?.Invoke();
		}, null);
	}

	public void ConcedeButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		if (_allowMatchConcession)
		{
			this.ConcedeMatchRequestedHandlers?.Invoke();
		}
		else
		{
			this.ConcedeGameRequestedHandlers?.Invoke();
		}
	}

	public void SkipTutorialButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/Gameplay/SkipTutorial_Confirm_Title"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/Gameplay/SkipTutorial_Confirm"), delegate
		{
			this.SkipTutorialRequestedHandlers?.Invoke();
		}, null);
	}

	public void SkipOnboardingButton_OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/Gameplay/UnlockGameModesWarningTitle"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/Gameplay/UnlockGameModesWarningBody"), ShowSkipOnboardingConfirmSystemMessage, null);
	}

	private void ShowSkipOnboardingConfirmSystemMessage()
	{
		GlobalCoroutineExecutor executor = Pantry.Get<GlobalCoroutineExecutor>();
		SystemMessageManager.Instance.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/Gameplay/SkipOnboarding"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Settings/Gameplay/SkipPlayModesConfirm"), delegate
		{
			executor.StartGlobalCoroutine(this.SkipOnboardingRequestedHandlers?.Invoke().AsCoroutine());
		}, null);
	}

	public void OnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	private void GoToPanel(SettingsMenuPanel targetPanel)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		foreach (SettingsPanel settingsPanel in _settingsPanels)
		{
			bool flag = settingsPanel.Panel == targetPanel;
			if (settingsPanel.Panel.gameObject.activeSelf != flag)
			{
				if (flag)
				{
					settingsPanel.Panel.ShowPanel();
				}
				else
				{
					settingsPanel.Panel.HidePanel();
				}
				settingsPanel.Panel.gameObject.SetActive(flag);
			}
		}
		MainMenuPanel.UpdateActive(targetPanel == null);
		MainMenuBackground.UpdateActive(targetPanel == null);
		SubMenuBackground.UpdateActive(targetPanel != null);
		_activePanel = targetPanel;
	}

	public void OnBack(ActionContext context)
	{
		OnCloseClicked();
	}
}
