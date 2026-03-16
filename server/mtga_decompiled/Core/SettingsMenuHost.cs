using System;
using System.Threading.Tasks;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using AssetLookupTree.Payloads.Wrapper;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Code.Input;
using Core.Meta.MainNavigation.Challenge;
using Core.Shared.Code.Connection;
using Core.Shared.Code.Utilities;
using MTGA.KeyboardManager;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.Providers;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtgo.Gre.External.Messaging;

public class SettingsMenuHost : IKeyDownSubscriber, IKeySubscriber, IKeyUpSubscriber, IBackActionHandler
{
	private readonly LoggingConfig _loggingConfig;

	private readonly IAccountClient _accountClient;

	private ISocialManager _socialManager;

	private SettingsMenu _menuInstance;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private MatchManager _matchManager;

	private AssetLookupSystem _assetLookupSystem;

	private ISetMetadataProvider _setMetadataProvider;

	private IBILogger _biLogger;

	private Func<NPEState> _getNPEState;

	private PVPChallengeController _challengeController;

	private bool _isInGame;

	public PriorityLevelEnum Priority => PriorityLevelEnum.SettingsMenu;

	public event System.Action CurrencyChangedHandlers;

	public static SettingsMenuHost Create()
	{
		return new SettingsMenuHost(LoggingUtils.LoggingConfig, Pantry.Get<IAccountClient>(), Pantry.Get<ISocialManager>(), Pantry.Get<KeyboardManager>(), Pantry.Get<IActionSystem>(), Pantry.Get<MatchManager>(), Pantry.Get<AssetLookupManager>().AssetLookupSystem, Pantry.Get<NPEState>, Pantry.Get<ISetMetadataProvider>());
	}

	public SettingsMenuHost(LoggingConfig loggingConfig, IAccountClient accountClient, ISocialManager socialManager, KeyboardManager keyboardManager, IActionSystem actionSystem, MatchManager matchManager, AssetLookupSystem assetLookupSystem, Func<NPEState> getNPEState, ISetMetadataProvider setMetadataProvider)
	{
		_loggingConfig = loggingConfig;
		_accountClient = accountClient;
		_socialManager = socialManager;
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		_matchManager = matchManager;
		_assetLookupSystem = assetLookupSystem;
		_getNPEState = getNPEState;
		_setMetadataProvider = setMetadataProvider;
		_matchManager.SideboardSubmitted += OnSideboardSubmitted;
		_matchManager.MatchStateChanged += OnMatchStateChanged;
		_matchManager.MatchCompleted += OnMatchCompleted;
		_actionSystem.PushFocus(this);
	}

	public void Destroy()
	{
		if (_matchManager != null)
		{
			_matchManager.SideboardSubmitted -= OnSideboardSubmitted;
			_matchManager.MatchStateChanged -= OnMatchStateChanged;
			_matchManager.MatchCompleted -= OnMatchCompleted;
			_matchManager = null;
		}
		_actionSystem.PopFocus(this);
	}

	public void SetSocialClient(ISocialManager socialManager)
	{
		_socialManager = socialManager;
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (curr == KeyCode.Escape)
		{
			Close();
		}
		return true;
	}

	public bool HandleKeyUp(KeyCode curr, Modifiers mods)
	{
		return true;
	}

	public void OnBack(ActionContext context)
	{
		Open();
	}

	private SettingsMenu GetOrCreateMenu()
	{
		if (!_menuInstance)
		{
			_menuInstance = UnityEngine.Object.FindObjectOfType<SettingsMenu>();
		}
		if (!_menuInstance)
		{
			_assetLookupSystem.Blackboard.Clear();
			SettingsMenuPrefab payload = _assetLookupSystem.TreeLoader.LoadTree<SettingsMenuPrefab>(returnNewTree: false).GetPayload(_assetLookupSystem.Blackboard);
			_menuInstance = AssetLoader.Instantiate(payload.Prefab);
			_menuInstance.Init(_loggingConfig, _socialManager, _challengeController);
			_menuInstance.DestroyedHandlers += OnSettingsMenuDestroyed;
			_menuInstance.EnableDetailedLogsRequestedHandlers += OnEnableDetailedLogsRequested;
			_menuInstance.DisableDetailedLogsRequestedHandlers += OnDisableDetailedLogsRequested;
			_menuInstance.EnableBlockFriendRequestsRequestedHandlers += OnEnableBlockFriendRequestsRequested;
			_menuInstance.DisableBlockFriendRequestsRequestedHandlers += OnDisableBlockFriendRequestsRequested;
			_menuInstance.EnableBlockNonFriendChallengesHandlers += OnEnableBlockNonFriendChallengesRequested;
			_menuInstance.DisableBlockNonFriendChallengesHandlers += OnDisableBlockNonFriendChallengesRequested;
			_menuInstance.CloseRequestedHandlers += OnCloseRequested;
			_menuInstance.LogoutRequestedHandlers += OnLogoutRequested;
			_menuInstance.ExitApplicationRequestedHandlers += OnExitApplicationRequested;
			_menuInstance.SkipTutorialRequestedHandlers += OnSkipTutorialRequested;
			_menuInstance.SkipOnboardingRequestedHandlers += OnSkipOnboardingRequested;
			_menuInstance.ConcedeGameRequestedHandlers += OnConcedeGameRequested;
			_menuInstance.ConcedeMatchRequestedHandlers += OnConcedeMatchRequested;
			_menuInstance.Close();
		}
		return _menuInstance;
	}

	private void OnSideboardSubmitted()
	{
		Close();
	}

	private void OnMatchStateChanged(MatchState matchState)
	{
		Close();
	}

	private void OnMatchCompleted()
	{
		Close();
	}

	private void OnSettingsMenuDestroyed()
	{
		if ((bool)_menuInstance)
		{
			_menuInstance.DestroyedHandlers -= OnSettingsMenuDestroyed;
			_menuInstance.EnableDetailedLogsRequestedHandlers -= OnEnableDetailedLogsRequested;
			_menuInstance.DisableDetailedLogsRequestedHandlers -= OnDisableDetailedLogsRequested;
			_menuInstance.EnableBlockFriendRequestsRequestedHandlers -= OnEnableBlockFriendRequestsRequested;
			_menuInstance.DisableBlockFriendRequestsRequestedHandlers -= OnDisableBlockFriendRequestsRequested;
			_menuInstance.EnableBlockNonFriendChallengesHandlers -= OnEnableBlockNonFriendChallengesRequested;
			_menuInstance.DisableBlockNonFriendChallengesHandlers -= OnDisableBlockNonFriendChallengesRequested;
			_menuInstance.CloseRequestedHandlers -= OnCloseRequested;
			_menuInstance.LogoutRequestedHandlers -= OnLogoutRequested;
			_menuInstance.ExitApplicationRequestedHandlers -= OnExitApplicationRequested;
			_menuInstance.SkipTutorialRequestedHandlers -= OnSkipTutorialRequested;
			_menuInstance.SkipOnboardingRequestedHandlers -= OnSkipOnboardingRequested;
			_menuInstance.ConcedeGameRequestedHandlers -= OnConcedeGameRequested;
			_menuInstance.ConcedeMatchRequestedHandlers -= OnConcedeMatchRequested;
		}
		_menuInstance = null;
	}

	private void OnConcedeGameRequested()
	{
		if (_matchManager != null && _matchManager.GreInterface != null)
		{
			_matchManager.GreInterface.ConcedeGame();
		}
		Close();
	}

	private void OnConcedeMatchRequested()
	{
		if (_matchManager != null && _matchManager.GreInterface != null)
		{
			_matchManager.GreInterface.ConcedeMatch();
		}
		Close();
	}

	private void OnEnableDetailedLogsRequested()
	{
		MDNPlayerPrefs.SetUseVerboseLogs(newValue: true);
		_loggingConfig.VerboseLogs = true;
	}

	private void OnDisableDetailedLogsRequested()
	{
		MDNPlayerPrefs.SetUseVerboseLogs(newValue: false);
		_loggingConfig.VerboseLogs = false;
	}

	private void OnEnableBlockFriendRequestsRequested()
	{
		_socialManager.ToggleAutoDeclineFriendInviteIncoming(shouldDecline: true);
	}

	private void OnDisableBlockFriendRequestsRequested()
	{
		_socialManager.ToggleAutoDeclineFriendInviteIncoming(shouldDecline: false);
	}

	private void OnEnableBlockNonFriendChallengesRequested()
	{
		_challengeController.ToggleBlockNonFriendChallengesIncoming(shouldBlock: true);
	}

	private void OnDisableBlockNonFriendChallengesRequested()
	{
		_challengeController.ToggleBlockNonFriendChallengesIncoming(shouldBlock: false);
	}

	private void OnCloseRequested()
	{
		Close();
	}

	private void OnLogoutRequested()
	{
		Close();
		Pantry.Get<PVPChallengeController>().HandleLogout();
		Pantry.Get<FrontDoorConnectionManager>().LogoutAndRestartGame("Setting log out button");
	}

	private void OnSkipTutorialRequested()
	{
		if (_isInGame)
		{
			_getNPEState?.Invoke().QueueUpSkipTutorialFromInGame();
		}
		else
		{
			NPEStitcherScene nPEStitcherScene = UnityEngine.Object.FindObjectOfType<NPEStitcherScene>();
			if (nPEStitcherScene != null)
			{
				nPEStitcherScene.SkipTutorialRequested();
			}
			else
			{
				Debug.LogError("[NPE] Attempted to skip tutorial, but we weren't in a game or in the stitcher scene! " + SceneManager.GetActiveScene().name);
			}
		}
		_menuInstance.GoToMainMenu();
		Close();
	}

	private async Task OnSkipOnboardingRequested()
	{
		await WrapperController.Instance.SparkyTourState.SkipTour();
		if (!_isInGame)
		{
			PAPA.SceneLoading.LoadWrapperScene(new HomePageContext
			{
				ForceReload = true
			});
		}
		_menuInstance.GoToMainMenu();
		Close();
	}

	private void OnExitApplicationRequested()
	{
		if (WrapperController.Instance != null)
		{
			WrapperController.Instance.NavBarController.PressedExitButton();
		}
		else
		{
			_ = Application.isEditor;
			Application.Quit();
		}
		Close();
	}

	public async Task Open()
	{
		if (_matchManager == null || _matchManager.MatchState == MatchState.MatchComplete)
		{
			return;
		}
		IAccountClient accountClient = _accountClient;
		bool isLoggedIn = accountClient != null && accountClient.CurrentLoginState == LoginState.FullyRegisteredLogin;
		MatchState matchState = _matchManager.MatchState;
		MatchSceneManager.SubScene? subScene = MatchSceneManager.Instance?.Current;
		bool allowGameConcession = matchState == MatchState.GameInProgress && (subScene == MatchSceneManager.SubScene.GameReady || subScene == MatchSceneManager.SubScene.DuelScene);
		bool allowMatchConcession = (matchState == MatchState.GameComplete && subScene == MatchSceneManager.SubScene.DuelScene) || (matchState == MatchState.GameComplete && subScene == MatchSceneManager.SubScene.WaitingForGame);
		NPEState npeState = _getNPEState();
		int num;
		if (isLoggedIn)
		{
			if (npeState != null && npeState.TutorialState == NPEState.TutorialStates.Underway)
			{
				num = ((npeState != null && !npeState.SkipTutorialButtonLocked) ? 1 : 0);
				goto IL_014b;
			}
		}
		num = 0;
		goto IL_014b;
		IL_01fc:
		int num2;
		bool allowSkipOnboarding = (byte)num2 != 0;
		bool allowLogout = isLoggedIn && !allowGameConcession && !allowMatchConcession;
		bool allowExit = !allowGameConcession && !allowMatchConcession && !PlatformUtils.IsHandheld();
		int num3;
		if (!Debug.isDebugBuild)
		{
			IAccountClient accountClient2 = _accountClient;
			num3 = ((accountClient2 != null && accountClient2.AccountInformation?.HasRole_Debugging() == true) ? 1 : 0);
		}
		else
		{
			num3 = 1;
		}
		bool allowDebug = (byte)num3 != 0;
		_isInGame = allowMatchConcession || allowGameConcession;
		_challengeController = Pantry.Get<PVPChallengeController>();
		_keyboardManager.Subscribe(this);
		_assetLookupSystem.Blackboard.Clear();
		_assetLookupSystem.Blackboard.SetCode = _setMetadataProvider.LastPublishedMajorSet;
		SettingsBackgroundPayload payload = _assetLookupSystem.TreeLoader.LoadTree<SettingsBackgroundPayload>().GetPayload(_assetLookupSystem.Blackboard);
		SettingsMenu orCreateMenu = GetOrCreateMenu();
		if (!orCreateMenu.TrySetBackgroundImages(payload))
		{
			SimpleLog.LogErrorFormat("Encountered issue when trying to set SettingsMenu background images. Please ensure SettingsBackgroundPayload returns a valid payload for LastPublishedMajorSet {0}", _setMetadataProvider.LastPublishedMajorSet);
		}
		bool allowSkipTutorial;
		orCreateMenu.Open(allowLogout, allowExit, allowGameConcession, allowMatchConcession, allowSkipTutorial, allowSkipOnboarding, allowDebug);
		_actionSystem.PushFocus(orCreateMenu, IActionSystem.Priority.Settings);
		return;
		IL_014b:
		allowSkipTutorial = (byte)num != 0;
		bool flag = true;
		if (WrapperController.Instance != null && WrapperController.Instance.SparkyTourState != null)
		{
			flag = await WrapperController.Instance.SparkyTourState.EventsUnlocked();
		}
		if (isLoggedIn)
		{
			if (npeState != null && npeState.TutorialState == NPEState.TutorialStates.Completed)
			{
				num2 = ((!flag) ? 1 : 0);
				goto IL_01fc;
			}
		}
		num2 = 0;
		goto IL_01fc;
	}

	public void Close()
	{
		SettingsMenu orCreateMenu = GetOrCreateMenu();
		if (orCreateMenu == null)
		{
			_keyboardManager.Unsubscribe(this);
			_actionSystem.PopFocus(orCreateMenu);
		}
		else if (orCreateMenu.IsMainPanelActive)
		{
			orCreateMenu.Close();
			_keyboardManager.Unsubscribe(this);
			_actionSystem.PopFocus(orCreateMenu);
		}
		else
		{
			orCreateMenu.GoToMainMenu();
		}
	}

	public bool IsOpen()
	{
		return _menuInstance?.IsOpen ?? false;
	}

	public bool IsGameObjectActive()
	{
		return GetOrCreateMenu().gameObject.activeSelf;
	}
}
