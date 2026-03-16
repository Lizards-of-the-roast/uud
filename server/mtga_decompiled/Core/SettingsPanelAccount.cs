using System;
using Assets.Core.Meta.Utilities;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Challenge;
using MTGA.Social;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Mtga.Store;
using Wotc.Mtga.Loc;

public sealed class SettingsPanelAccount : SettingsMenuPanel
{
	[SerializeField]
	private CustomButton _backButton;

	[SerializeField]
	private CustomButton _restorePurchasesButton;

	[Header("Trackers")]
	[SerializeField]
	private Toggle _trackerToggle;

	[Header("Block Friend Requests")]
	[SerializeField]
	private GameObject _blockFriendRequestsToggleParent;

	[SerializeField]
	private Toggle _blockFriendRequestsToggle;

	[Header("Block non-Friend Challenge Requests")]
	[SerializeField]
	private GameObject _blockNonFriendChallengeRequestsToggleParent;

	[SerializeField]
	private Toggle _blockNonFriendChallengeRequestsToggle;

	private LoggingConfig _loggingConfig;

	private ISocialManager _socialManager;

	private PVPChallengeController _challengeController;

	public event Action<bool> DetailedLogsToggled;

	public event Action<bool> BlockFriendRequestsToggled;

	public event Action<bool> BlockNonFriendChallengesToggled;

	private static void SafeAddOnClickLink(Button button, string locKey)
	{
		if (button != null)
		{
			button.onClick.AddListener(delegate
			{
				ClickedLink(Languages.ActiveLocProvider.GetLocalizedText(locKey));
			});
		}
	}

	private void Awake()
	{
		_backButton.OnClick.AddListener(delegate
		{
			_settingsMenu.GoToMainMenu();
		});
		_trackerToggle.onValueChanged.AddListener(OnTrackerChanged);
		_blockFriendRequestsToggle.onValueChanged.AddListener(OnBlockFriendRequestsChanged);
		_blockNonFriendChallengeRequestsToggle.onValueChanged.AddListener(OnBlockNonFriendChallengeRequestsChanged);
	}

	private void SetupRestorePurchasesButton()
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			_restorePurchasesButton.gameObject.SetActive(value: false);
			return;
		}
		WrapperController instance = WrapperController.Instance;
		if (instance != null && instance.Store is GeneralStoreManager generalStoreManager)
		{
			_restorePurchasesButton.OnClick.AddListener(generalStoreManager.RestoreTransactions);
		}
	}

	public void Init(LoggingConfig loggingConfig, ISocialManager socialManager, PVPChallengeController challengeController)
	{
		_loggingConfig = loggingConfig;
		_socialManager = socialManager;
		_challengeController = challengeController;
		_blockFriendRequestsToggleParent.SetActive(_socialManager != null);
		_blockFriendRequestsToggle.isOn = _socialManager?.DeclineIncomingFriendRequests ?? false;
		PVPChallengeController challengeController2 = _challengeController;
		if (challengeController2 != null && challengeController2.CanSetBlockNonFriendChallenges)
		{
			_challengeController.GetBlockNonFriendChallengesIncoming().ThenOnMainThreadIfSuccess(delegate(bool result)
			{
				_blockNonFriendChallengeRequestsToggleParent.SetActive(value: true);
				_blockNonFriendChallengeRequestsToggle.isOn = result;
			});
		}
		else
		{
			_blockNonFriendChallengeRequestsToggleParent.SetActive(value: false);
		}
	}

	public override void ShowPanel()
	{
		base.ShowPanel();
		_blockFriendRequestsToggleParent.SetActive(_socialManager?.IsSocialEnabled ?? false);
		_trackerToggle.isOn = _loggingConfig.VerboseLogs;
	}

	private static void ClickedLink(string url)
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_generic_click, AudioManager.Default);
		UrlOpener.OpenURL(url);
	}

	public void HoverAudio()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, AudioManager.Default);
	}

	public void OnTrackerChanged(bool val)
	{
		this.DetailedLogsToggled?.Invoke(val);
	}

	public void OnBlockFriendRequestsChanged(bool isOn)
	{
		this.BlockFriendRequestsToggled?.Invoke(isOn);
	}

	public void OnBlockNonFriendChallengeRequestsChanged(bool isOn)
	{
		this.BlockNonFriendChallengesToggled?.Invoke(isOn);
	}

	private void OnDestroy()
	{
		_backButton.OnClick.RemoveAllListeners();
		_trackerToggle.onValueChanged.RemoveAllListeners();
		_blockFriendRequestsToggle.onValueChanged.RemoveAllListeners();
	}
}
