using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.Code.Input;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Challenge;
using Core.Meta.MainNavigation.Tournaments;
using MTGA.KeyboardManager;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.PrivateGame;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class SocialUI : MonoBehaviour, IKeyUpSubscriber, IKeySubscriber, IKeyDownSubscriber, INextActionHandler, IPreviousActionHandler, IAcceptActionHandler, IBackActionHandler, IActionBlocker
{
	private enum AnimatorCornerIconState
	{
		Disabled,
		Offline,
		Online,
		Busy,
		HotChallengeOn,
		HotOn
	}

	private FriendsWidget _friendsWidget;

	private ChatWindow _chatWindow;

	private FriendInvitePanel _friendInvitePanel;

	private SocialMessagesView _friendsNotificationMessageView;

	private SocialFocusListener _socialFocusListener;

	[Header("Social Entities List Widget")]
	[SerializeField]
	private RectTransform _topSortingRoot;

	[SerializeField]
	private RectTransform _safeZoneRoot;

	[Header("Social Corner Icon")]
	[SerializeField]
	private GameObject _cornerIcon;

	[SerializeField]
	private Transform _cornerIconWrapperParent;

	[SerializeField]
	private Transform _cornerIconDuelSceneParent;

	[SerializeField]
	private CustomButton _buttonOpenFriendsList;

	[SerializeField]
	private Animator _animatorCornerIcon;

	[SerializeField]
	private TablesCornerUI tablesCornerUI;

	[SerializeField]
	private TMP_Text _labelOnlineFriendCount;

	[SerializeField]
	private TooltipTrigger _tooltipCornerIcon;

	private static readonly int VisibleHash = Animator.StringToHash("Visible");

	private static readonly int ChatHash = Animator.StringToHash("Chat");

	private static readonly int WidgetHash = Animator.StringToHash("Widget");

	private static readonly int ToggleNumber = Animator.StringToHash("ShowNu");

	private static readonly int IconStateHash = Animator.StringToHash("IconState");

	private Animator _animator;

	private Canvas _canvas;

	private bool _visible = true;

	private bool _disabled;

	private bool _toggleShowSocialEntitiesList;

	private readonly List<SocialMessage> _pendingMessages = new List<SocialMessage>();

	private ISocialManager _socialManager;

	private IActionSystem _actions;

	private PVPChallengeController _challengeController;

	private ITournamentController _tournamentController;

	private KeyboardManager _keyboardManager;

	private AssetLookupSystem _assetLookupSystem;

	private static SocialUI _instance;

	private DateTime _timeInvitePanelClosed = DateTime.UtcNow;

	public TablesCornerUI TablesCornerUI => tablesCornerUI;

	public Animator AnimatorCornerIcon => _animatorCornerIcon;

	private SceneLoader _sceneLoader => SceneLoader.GetSceneLoader();

	private ChatManager _chatManager => _socialManager.ChatManager;

	public bool SocialEntitiesListVisible { get; private set; }

	public bool ChatVisible { get; private set; }

	public bool InvitePanelVisible { get; private set; }

	public bool Disabled
	{
		get
		{
			return _disabled;
		}
		set
		{
			if (_disabled == value)
			{
				return;
			}
			_disabled = value;
			_buttonOpenFriendsList.enabled = !_disabled;
			_tooltipCornerIcon.IsActive = _disabled;
			_tooltipCornerIcon.TooltipData.Text = Languages.ActiveLocProvider.GetLocalizedText("Social/Errors/KillSwitchEnabled_Desc");
			if (_animatorCornerIcon != null && _animatorCornerIcon.isActiveAndEnabled)
			{
				if (_disabled)
				{
					UpdateCornerIconAnimatorState();
					Minimize();
				}
				else
				{
					UpdatePresenceStatus(_socialManager.LocalPlayer.Status);
				}
			}
		}
	}

	public bool MissingAssetBundleRefrences => _animator.runtimeAnimatorController == null;

	public PriorityLevelEnum Priority => PriorityLevelEnum.Social;

	public void Init(ISocialManager socialManager, KeyboardManager keyboardManager, AssetLookupSystem assetLookupSystem, IActionSystem actionSystem)
	{
		if (socialManager != null)
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
			if (_socialManager != null)
			{
				UnInit();
			}
			_socialManager = socialManager;
			_keyboardManager = keyboardManager;
			_assetLookupSystem = assetLookupSystem;
			_actions = actionSystem;
			_instance._socialFocusListener = new SocialFocusListener(actionSystem, this);
			_socialManager.ConnectionChanged += UpdateConnectionState;
			_socialManager.FriendsChanged += HandleFriendsChanged;
			_socialManager.InvitesOutgoingChanged += HandleInvitesOutgoingChanged;
			_socialManager.InviteIncomingAdded += delegate
			{
				AudioManager.PlayAudio("sfx_ui_friends_incoming_request", base.gameObject);
			};
			_socialManager.InvitesIncomingChanged += HandleInvitesIncomingChanged;
			_socialManager.BlocksChanged += HandleBlocksChanged;
			_socialManager.SocialException += HandleSocialException;
			_socialManager.UnknownException += HandleUnknownException;
			_socialManager.SocialEnabledChanged += HandleSocialEnabledChanged;
			_socialManager.LocalPresenceStatusChanged += HandlePresenceStatusChanged;
			Disabled = !_socialManager.IsSocialEnabled;
			if (_socialManager.Connected)
			{
				UpdateConnectionState();
				HandleFriendsChanged();
			}
			_chatManager.NotificationAlert += NotificationAlert;
			_chatManager.NotificationCancel += NotificationCancel;
			_chatManager.ConversationSelected += ConversationSelected;
			_keyboardManager.Subscribe(this);
			tablesCornerUI?.Init(_actions, this);
			_challengeController = Pantry.Get<PVPChallengeController>();
			_challengeController.RegisterForChallengeChanges(HandleChallengeDataUpdate);
			HandleFriendsChanged();
			UpdateConnectionState();
		}
	}

	private void Awake()
	{
		_instance = this;
		_animator = GetComponent<Animator>();
		_canvas = GetComponent<Canvas>();
		_canvas.worldCamera = CurrentCamera.Value;
		CurrentCamera.OnCurrentCameraUpdated = (Action<Camera>)Delegate.Combine(CurrentCamera.OnCurrentCameraUpdated, new Action<Camera>(UpdateCurrentCamera));
		PAPA.SceneLoading.OnWrapperSceneLoaded = (Action<object>)Delegate.Combine(PAPA.SceneLoading.OnWrapperSceneLoaded, new Action<object>(OnWrapperSceneLoaded));
		PAPA.SceneLoading.OnDuelSceneLoaded = (Action<LoadSceneMode>)Delegate.Combine(PAPA.SceneLoading.OnDuelSceneLoaded, new Action<LoadSceneMode>(OnDuelSceneLoaded));
		_buttonOpenFriendsList.OnClick.AddListener(ToggleFriendsWithButton);
		InvitePanelVisible = false;
		Minimize();
		if (_socialManager != null)
		{
			UpdateSocialEntityList();
		}
		UpdateCornerIconAnimatorState();
	}

	private void OnEnable()
	{
		if (_socialManager == null)
		{
			UpdatePresenceStatus(PresenceStatus.Offline);
			return;
		}
		UpdateConnectionState();
		_animator.SetBool(VisibleHash, _visible);
		UpdateNotifications();
		UpdateCornerIconAnimatorState();
	}

	private void Update()
	{
		if (!_disabled && ((!PlatformUtils.IsHandheld() && CustomInputModule.PointerIsHeldDown()) || CustomInputModule.IsRightClick()) && SocialEntitiesListVisible && !ChatVisible && _friendInvitePanel == null && !((RectTransform)_friendsWidget.transform).GetMouseOver())
		{
			Minimize();
		}
	}

	private void UnInit()
	{
		if (_socialManager != null)
		{
			_chatManager.NotificationAlert -= NotificationAlert;
			_chatManager.NotificationCancel -= NotificationCancel;
			_chatManager.ConversationSelected -= ConversationSelected;
			_socialManager.ConnectionChanged -= UpdateConnectionState;
			_socialManager.FriendsChanged -= HandleFriendsChanged;
			_socialManager.InvitesOutgoingChanged -= HandleInvitesOutgoingChanged;
			_socialManager.InvitesIncomingChanged -= HandleInvitesIncomingChanged;
			_socialManager.BlocksChanged -= HandleBlocksChanged;
			_socialManager.SocialException -= HandleSocialException;
			_socialManager.UnknownException -= HandleUnknownException;
			_socialManager.SocialEnabledChanged -= HandleSocialEnabledChanged;
			_socialManager.LocalPresenceStatusChanged -= HandlePresenceStatusChanged;
			_socialManager = null;
		}
		Dictionary<Guid, PVPChallengeData> dictionary = null;
		if (_challengeController != null)
		{
			dictionary = _challengeController.GetAllChallenges();
		}
		if (dictionary != null && dictionary.Any((KeyValuePair<Guid, PVPChallengeData> pair) => pair.Value.ChallengePlayers.ContainsKey(pair.Value.LocalPlayerId)) && _sceneLoader != null && _sceneLoader.CurrentContentType == NavContentType.Home && _sceneLoader.CurrentNavContent is HomePageContentController homePageContentController)
		{
			homePageContentController.ChallengeBladeController.Hide();
		}
	}

	private void OnDestroy()
	{
		if (_instance == this)
		{
			_instance = null;
		}
		UnInit();
		_keyboardManager?.Unsubscribe(this);
		PAPA.SceneLoading.OnWrapperSceneLoaded = (Action<object>)Delegate.Remove(PAPA.SceneLoading.OnWrapperSceneLoaded, new Action<object>(OnWrapperSceneLoaded));
		PAPA.SceneLoading.OnDuelSceneLoaded = (Action<LoadSceneMode>)Delegate.Remove(PAPA.SceneLoading.OnDuelSceneLoaded, new Action<LoadSceneMode>(OnDuelSceneLoaded));
		CurrentCamera.OnCurrentCameraUpdated = (Action<Camera>)Delegate.Remove(CurrentCamera.OnCurrentCameraUpdated, new Action<Camera>(UpdateCurrentCamera));
		_challengeController?.UnRegisterForChallengeChanges(HandleChallengeDataUpdate);
		_buttonOpenFriendsList.OnClick.RemoveListener(ToggleFriendsWithButton);
		_socialFocusListener.CleanUp();
		_socialFocusListener = null;
	}

	private void OnApplicationQuit()
	{
		_socialManager?.Destroy();
	}

	public void ShowSocialEntitiesList()
	{
		if (!SocialEntitiesListVisible)
		{
			if (!_actions.IsCurrentFocus(this))
			{
				_actions.PushFocus(this);
			}
			SocialEntitiesListVisible = true;
			ChatVisible = false;
			_animator.SetBool(WidgetHash, SocialEntitiesListVisible);
			_animator.SetBool(ChatHash, ChatVisible);
			LoadFriendsWidget();
			_animatorCornerIcon.SetBool(ToggleNumber, value: false);
			_chatManager.SelectConversation(null);
			if (_friendsWidget == null || _friendsWidget.FriendsListScroll.verticalNormalizedPosition >= 1f)
			{
				UpdateSocialEntityList();
			}
			UpdateCornerIconAnimatorState();
			HideTableIcon();
			_socialManager.BIMessage_FriendsListOpened();
		}
	}

	public void ShowChatWindow(SocialEntity chatFriend = null)
	{
		if (!_actions.IsCurrentFocus(this))
		{
			_actions.PushFocus(this);
		}
		if (!ChatVisible)
		{
			ChatVisible = true;
			SocialEntitiesListVisible = PlatformUtils.IsHandheld();
			TearDownfriendsNotificationView();
			_animator.SetBool(WidgetHash, SocialEntitiesListVisible);
			_animator.SetBool(ChatHash, ChatVisible);
			_animatorCornerIcon.SetBool(ToggleNumber, value: false);
			UpdateCornerIconAnimatorState();
			HideTableIcon();
			if (!SocialEntitiesListVisible)
			{
				TeardownFriendsWidget();
			}
		}
		if (chatFriend == null)
		{
			_chatManager.SelectNextConversation();
		}
		else
		{
			_chatManager.SelectConversation(chatFriend);
		}
	}

	private void ShowTableIcon()
	{
		tablesCornerUI?.UpdateCornerIconActive(active: true);
	}

	private void HideTableIcon()
	{
		tablesCornerUI?.UpdateCornerIconActive(active: false);
	}

	public void SetVisible(bool visible)
	{
		if (_visible != visible)
		{
			_visible = visible;
			_animator.SetBool(VisibleHash, _visible);
			if (_visible)
			{
				HandleFriendsChanged();
				UpdateNotifications();
			}
		}
	}

	public void Minimize()
	{
		CloseFriendsAndChallengeWidget();
		CloseChat();
		ShowTableIcon();
		IActionSystem actions = _actions;
		if (actions != null && actions.IsCurrentFocus(this))
		{
			_actions.PopFocus(this);
		}
	}

	public void CloseFriendsAndChallengeWidget()
	{
		if (SocialEntitiesListVisible)
		{
			SocialEntitiesListVisible = false;
			_animator.SetBool(WidgetHash, SocialEntitiesListVisible);
			_animatorCornerIcon.SetBool(ToggleNumber, value: true);
			TeardownFriendsWidget();
			LoadFriendsNotificationView();
			UpdateCornerIconAnimatorState();
		}
	}

	public void CloseChat()
	{
		if (ChatVisible)
		{
			ChatVisible = false;
			_animator.SetBool(ChatHash, ChatVisible);
			_animatorCornerIcon.SetBool(ToggleNumber, value: true);
			_chatManager.SelectConversation(null);
			TeardownChatWindow();
			LoadFriendsNotificationView();
			UpdateCornerIconAnimatorState();
		}
	}

	private void LoadFriendsWidget()
	{
		TeardownFriendsWidget();
		TearDownfriendsNotificationView();
		string prefabPath = _assetLookupSystem.GetPrefabPath<FriendsWidgetPrefab, FriendsWidget>();
		_friendsWidget = AssetLoader.Instantiate<FriendsWidget>(prefabPath, _safeZoneRoot);
		_friendsWidget.transform.SetAsFirstSibling();
		bool bisInDuelScene = _sceneLoader?.IsInDuelScene ?? true;
		_friendsWidget.Init(this, _socialManager, _challengeController, _topSortingRoot, _actions, bisInDuelScene);
		UpdateConnectionState();
	}

	private void LoadFriendsNotificationView()
	{
		if (!PlatformUtils.IsHandheld())
		{
			TearDownfriendsNotificationView();
			string prefabPath = _assetLookupSystem.GetPrefabPath<FriendsNotificationPrefab, SocialMessagesView>();
			if (!string.IsNullOrEmpty(prefabPath))
			{
				_friendsNotificationMessageView = AssetLoader.Instantiate<SocialMessagesView>(prefabPath, _safeZoneRoot);
				_friendsNotificationMessageView.Init(this, _socialManager, _challengeController, _pendingMessages);
				_pendingMessages.Clear();
			}
		}
		else
		{
			Debug.Log("[Social] Friend notifications not shown on mobile");
		}
	}

	public void LoadFriendInvitePanel()
	{
		if (!_friendInvitePanel)
		{
			string prefabPath = _assetLookupSystem.GetPrefabPath<FriendInvitePanelPrefab, FriendInvitePanel>();
			_friendInvitePanel = AssetLoader.Instantiate<FriendInvitePanel>(prefabPath, base.transform);
			FriendInvitePanel friendInvitePanel = _friendInvitePanel;
			friendInvitePanel.Callback_OnSubmitInput = (Action<string>)Delegate.Combine(friendInvitePanel.Callback_OnSubmitInput, new Action<string>(HandleAddFriendSubmit));
			FriendInvitePanel friendInvitePanel2 = _friendInvitePanel;
			friendInvitePanel2.Callback_OnClose = (Action)Delegate.Combine(friendInvitePanel2.Callback_OnClose, new Action(HandleAddFriendClose));
			AudioManager.PlayAudio("sfx_ui_friends_click", _animatorCornerIcon.gameObject);
			_friendInvitePanel.Show(_socialManager.LocalPlayer.FullName);
			InvitePanelVisible = true;
		}
	}

	private void TearDownfriendsNotificationView()
	{
		if ((bool)_friendsNotificationMessageView)
		{
			UnityEngine.Object.Destroy(_friendsNotificationMessageView.gameObject);
		}
	}

	private void TeardownFriendsWidget()
	{
		if ((bool)_friendsWidget)
		{
			UnityEngine.Object.Destroy(_friendsWidget.gameObject);
		}
	}

	private void ConversationSelected(Conversation prevConversation, Conversation currentConversation)
	{
		if (currentConversation == null && PlatformUtils.IsDesktop())
		{
			Minimize();
			return;
		}
		LoadChatWindow();
		_chatWindow.ConversationSelected(prevConversation, currentConversation);
	}

	private void LoadChatWindow()
	{
		if (!_chatWindow)
		{
			string prefabPath = _assetLookupSystem.GetPrefabPath<ChatWindowPrefab, ChatWindow>();
			_chatWindow = AssetLoader.Instantiate<ChatWindow>(prefabPath, _safeZoneRoot);
			_chatWindow.transform.SetAsFirstSibling();
			_chatWindow.Init(this, _socialManager, _challengeController, _actions);
		}
	}

	private void TeardownChatWindow()
	{
		if ((bool)_chatWindow)
		{
			_chatWindow.Hide();
		}
	}

	private void ToggleFriendsWithButton()
	{
		AudioManager.PlayAudio((!SocialEntitiesListVisible) ? "sfx_ui_friends_open" : "sfx_ui_friends_close", _instance._animatorCornerIcon.gameObject);
		if (!SocialEntitiesListVisible && !ChatVisible)
		{
			_instance._toggleShowSocialEntitiesList = true;
			ShowSocialEntitiesList();
			if (_sceneLoader != null && _sceneLoader.CurrentContentType == NavContentType.Home && _sceneLoader.CurrentNavContent is HomePageContentController homePageContentController)
			{
				homePageContentController.ObjectivesPanel?.CloseAllObjectivePopups();
			}
		}
		else
		{
			Minimize();
		}
		_toggleShowSocialEntitiesList = false;
	}

	public void CloseFriendsTopBarDismissButton()
	{
		AudioManager.PlayAudio("sfx_ui_friends_close", _animatorCornerIcon.gameObject);
		Minimize();
	}

	public void UpdateSocialEntityList()
	{
		if ((bool)_friendsWidget)
		{
			_friendsWidget.UpdateSocialEntityList();
		}
		if (!_socialManager.Connected || _socialManager.LocalPlayer.Status == PresenceStatus.Offline)
		{
			_labelOnlineFriendCount.text = string.Empty;
			return;
		}
		_labelOnlineFriendCount.text = _socialManager.Friends.Count((SocialEntity f) => f.IsOnline).ToString();
	}

	public void UpdateChallengeEntityList()
	{
		if ((bool)_friendsWidget)
		{
			_friendsWidget.UpdateChallengeList();
		}
	}

	private void UpdateNotifications()
	{
		if (_socialManager == null)
		{
			return;
		}
		LoadFriendsNotificationView();
		foreach (SocialMessage notification in _chatManager.Notifications)
		{
			NotificationAlert(notification);
		}
	}

	private void NotificationAlert(SocialMessage notification)
	{
		if (!base.gameObject.activeSelf)
		{
			return;
		}
		if (notification.Seen != MessageSeen.Unseen)
		{
			AudioManager.PlayAudio("sfx_ui_instant_message_receive", AudioManager.Default);
			return;
		}
		notification.Seen = MessageSeen.Notification;
		if (_socialManager.LocalPlayer.Status == PresenceStatus.Busy)
		{
			return;
		}
		TaskbarFlash.Flash();
		if (!SocialEntitiesListVisible)
		{
			switch (notification.Type)
			{
			case MessageType.Invite:
				if (_friendsNotificationMessageView == null || !_friendsNotificationMessageView.Initialized)
				{
					_pendingMessages.Add(notification);
				}
				break;
			case MessageType.Chat:
				if (!ChatVisible)
				{
					AudioManager.PlayAudio("sfx_ui_instant_message_1st_receive", base.gameObject);
				}
				break;
			}
			UpdateCornerIconAnimatorState();
		}
		else
		{
			if (notification.Type != MessageType.Chat)
			{
				return;
			}
			AudioManager.PlayAudio("sfx_ui_instant_message_1st_receive", AudioManager.Default);
			foreach (FriendTile activeFriendTile in _friendsWidget._activeFriendTiles)
			{
				activeFriendTile.UpdateStatus();
			}
		}
	}

	public void UpdateCornerIconAnimatorState()
	{
		if (_animatorCornerIcon == null)
		{
			return;
		}
		if (_disabled || _challengeController == null || _chatManager == null || _socialManager == null || !_socialManager.IsSocialEnabled)
		{
			_animatorCornerIcon.SetInteger(IconStateHash, 0);
			return;
		}
		if (!_socialManager.Connected || _socialManager.LocalPlayer.Status == PresenceStatus.Offline)
		{
			_animatorCornerIcon.SetInteger(IconStateHash, 1);
			return;
		}
		if (_socialManager.LocalPlayer.Status == PresenceStatus.Busy)
		{
			_animatorCornerIcon.SetInteger(IconStateHash, 3);
			return;
		}
		if (!SocialEntitiesListVisible && !ChatVisible)
		{
			if (_challengeController.IsChallengeEnabled() && _challengeController.GetAllChallenges().Any((KeyValuePair<Guid, PVPChallengeData> pair) => pair.Value.Invites.ContainsKey(pair.Value.LocalPlayerId)))
			{
				_animatorCornerIcon.SetInteger(IconStateHash, 4);
				return;
			}
			if (_chatManager.ConversationsUnseenCount() > 0)
			{
				_animatorCornerIcon.SetInteger(IconStateHash, 5);
				return;
			}
		}
		_animatorCornerIcon.SetInteger(IconStateHash, 2);
	}

	private void NotificationCancel(SocialMessage notification)
	{
		if (notification.Seen != MessageSeen.Unseen)
		{
			UpdateCornerIconAnimatorState();
		}
	}

	private void UpdatePresenceStatus(PresenceStatus currentStatus)
	{
		if (_friendsWidget != null)
		{
			_friendsWidget.UpdatePresenceStatus(currentStatus);
		}
		switch (currentStatus)
		{
		case PresenceStatus.Available:
			_tooltipCornerIcon.IsActive = _disabled;
			break;
		case PresenceStatus.Busy:
			_tooltipCornerIcon.IsActive = true;
			_tooltipCornerIcon.TooltipData.Text = Languages.ActiveLocProvider.GetLocalizedText("Social/Friends/ConnectionState/Busy_Tooltip");
			break;
		case PresenceStatus.Offline:
			_tooltipCornerIcon.IsActive = true;
			_tooltipCornerIcon.TooltipData.Text = Languages.ActiveLocProvider.GetLocalizedText("Social/Friends/ConnectionState/Offline_Tooltip");
			if (!_toggleShowSocialEntitiesList)
			{
				Minimize();
			}
			break;
		}
		UpdateCornerIconAnimatorState();
	}

	public void SubscribeToFriendChallenge(FriendTile widget)
	{
		widget.OpenPlayBlade = (Action<Guid>)Delegate.Combine(widget.OpenPlayBlade, new Action<Guid>(OpenPlayBlade));
	}

	public void OpenPlayBlade(Guid challengeId)
	{
		if (_challengeController.CanOpenChallenge(challengeId))
		{
			_instance.StartCoroutine(Coroutine_OpenPlayBlade_ViewFriendChallenge(challengeId));
			SceneLoader.GetSceneLoader().GetNavBar().RefreshButtons();
		}
	}

	public void AcceptChallenge(SocialEntity friend, Guid challengeId)
	{
		PVPChallengeData challenge = _challengeController.GetChallengeData(challengeId);
		if (challenge == null)
		{
			return;
		}
		Debug.Log("[Social] Accepting challenge from " + friend.DisplayName);
		if (!challenge.Invites.ContainsKey(challenge.LocalPlayerId))
		{
			return;
		}
		_challengeController.AcceptChallengeInvite(challenge.ChallengeId).ThenOnMainThread(delegate(Promise<PVPChallengeData> result)
		{
			if (result.Successful)
			{
				_instance.StartCoroutine(Coroutine_OpenPlayBlade_ViewFriendChallenge(challenge.ChallengeId));
				SceneLoader.GetSceneLoader().GetNavBar().RefreshButtons();
			}
		});
	}

	private void UpdateConnectionState(bool shutdownStyle = false)
	{
		Disabled = !_socialManager.IsSocialEnabled;
		if (!shutdownStyle)
		{
			if (_friendsWidget != null)
			{
				_friendsWidget.UsernameText.text = _socialManager.LocalPlayer.DisplayName;
			}
			UpdatePresenceStatus(_socialManager.LocalPlayer.Status);
		}
	}

	private void HandleSocialEnabledChanged(bool isSocialEnabled)
	{
		Debug.LogWarning($"[Social] Killswitch changed, isSocialEnabled: {isSocialEnabled}");
		UpdateConnectionState();
	}

	private void HandlePresenceStatusChanged(PresenceStatus previousStatus, PresenceStatus currentStatus)
	{
		UpdatePresenceStatus(currentStatus);
	}

	private void HandleUnknownException(Exception exception)
	{
		SystemMessageManager.Instance.ShowMessage((MTGALocalizedString)"MainNav/Store/Purchase_Error_Unknown_Title", new MTGALocalizedString
		{
			Key = "MainNav/Login/Default_Error_Message",
			Parameters = new Dictionary<string, string> { { "code", exception.Message } }
		}, (MTGALocalizedString)"MainNav/Popups/Modal/ModalOptions_Cancel", null, (MTGALocalizedString)"Boot/BootScene_Button_Retry", delegate
		{
			_socialManager.Connect();
		});
	}

	private void HandleSocialException(SocialError error)
	{
		switch (error.Error)
		{
		case "FRIEND LIMIT REACHED":
			SystemMessageManager.ShowSystemMessage((MTGALocalizedString)"Social/Errors/Wizards/FriendLimit/Title", (MTGALocalizedString)"Social/Errors/Wizards/FriendLimit/Body");
			return;
		case "FRIENDSHIP ALREADY EXISTS":
			SystemMessageManager.ShowSystemMessage((MTGALocalizedString)"Social/Errors/Wizards/AlreadFriends/Title", (MTGALocalizedString)"Social/Errors/Wizards/AlreadFriends/Body");
			return;
		case "SENDER AND RECEIVER MUST BE DIFFERENT":
			SystemMessageManager.ShowSystemMessage((MTGALocalizedString)"Social/Errors/Wizards/CannotFriendSelf/Title", (MTGALocalizedString)"Social/Errors/Wizards/CannotFriendSelf/Body");
			return;
		}
		HandleUnknownException(error.Exception);
		Debug.LogException(new Exception($"[Social] Social exception! {error.ErrorMessage} | {error.ErrorCode} {error.Error}"));
		UpdateConnectionState();
	}

	private void HandleChallengeDataUpdate(PVPChallengeData challengeData)
	{
		if ((bool)_friendsWidget)
		{
			_friendsWidget.UpdateChallengeList();
		}
		UpdateCornerIconAnimatorState();
	}

	public void ReadyForTournamentStart()
	{
		if (_tournamentController == null)
		{
			_tournamentController = Pantry.Get<ITournamentController>();
		}
		_tournamentController.TournamentPlayerReadyStart(_tournamentController.GetLatestTournamentId(), "", _tournamentController.GetTournamentDeckId(), "");
	}

	public void ReadyForTournamentRound()
	{
		if (_tournamentController == null)
		{
			_tournamentController = Pantry.Get<ITournamentController>();
		}
		_tournamentController.TournamentPlayerReadyRound(_tournamentController.GetLatestTournamentId(), "", "");
	}

	private void HandleFriendsChanged()
	{
		_socialManager.Friends.ForEach(delegate(SocialEntity entity)
		{
			entity.UpdateSortOrder();
		});
		_socialManager.Friends.Sort((SocialEntity a, SocialEntity b) => (a.SortOrder == b.SortOrder) ? string.Compare(a.DisplayName, b.DisplayName, StringComparison.CurrentCultureIgnoreCase) : b.SortOrder.CompareTo(a.SortOrder));
		UpdateSocialEntityList();
	}

	private void HandleAddFriendSubmit(string potentialFriend)
	{
		if (!string.IsNullOrWhiteSpace(potentialFriend))
		{
			_socialManager.SubmitFriendInviteOutgoing(potentialFriend);
		}
	}

	private void HandleAddFriendClose()
	{
		InvitePanelVisible = false;
		_timeInvitePanelClosed = DateTime.UtcNow;
		_friendInvitePanel = null;
	}

	private void HandleInvitesIncomingChanged()
	{
		_socialManager.InvitesIncoming.Sort((Invite a, Invite b) => string.Compare(a.PotentialFriend.DisplayName, b.PotentialFriend.DisplayName, StringComparison.CurrentCultureIgnoreCase));
		UpdateSocialEntityList();
	}

	private void HandleInvitesOutgoingChanged()
	{
		_socialManager.InvitesOutgoing.Sort((Invite a, Invite b) => string.Compare(a.PotentialFriend.DisplayName, b.PotentialFriend.DisplayName, StringComparison.CurrentCultureIgnoreCase));
		UpdateSocialEntityList();
	}

	public void HandleSectionChanged(bool headerVisible)
	{
		AudioManager.PlayAudio(headerVisible ? "sfx_ui_friends_group_open" : "sfx_ui_friends_group_close", _animatorCornerIcon.gameObject);
		UpdateSocialEntityList();
		UpdateChallengeEntityList();
	}

	private void HandleBlocksChanged()
	{
		_socialManager.Blocks.Sort((Block a, Block b) => string.Compare(a.BlockedPlayer.DisplayName, b.BlockedPlayer.DisplayName, StringComparison.CurrentCultureIgnoreCase));
		UpdateSocialEntityList();
	}

	private void UpdateCurrentCamera(Camera cam)
	{
		if (_canvas != null)
		{
			_canvas.worldCamera = cam;
		}
	}

	private void OnWrapperSceneLoaded(object panelContext)
	{
		if (_challengeController != null)
		{
			UpdateChallengeState();
		}
		if (_canvas != null)
		{
			_canvas.planeDistance = -15f;
		}
		base.gameObject.SetActive(value: true);
		if (_cornerIcon != null)
		{
			_cornerIcon.transform.SetParent((_cornerIconWrapperParent != null) ? _cornerIconWrapperParent : base.transform, worldPositionStays: false);
		}
	}

	private void OnDuelSceneLoaded(LoadSceneMode loadMode)
	{
		if (_challengeController != null)
		{
			UpdateChallengeState(bDuelScene: true);
		}
		if (_canvas != null)
		{
			_canvas.planeDistance = 10f;
		}
		base.gameObject.SetActive(value: true);
		if (_cornerIcon != null)
		{
			_cornerIcon.transform.SetParent((_cornerIconDuelSceneParent != null) ? _cornerIconDuelSceneParent : base.transform, worldPositionStays: false);
		}
	}

	private void UpdateChallengeState(bool bDuelScene = false)
	{
		if (_challengeController != null)
		{
			bool flag = _challengeController.IsChallengeEnabled();
			_challengeController.ChallengePermissionState = ((!flag) ? ChallengeDataProvider.ChallengePermissionState.Restricted_ChallengesKillswitched : ((!bDuelScene) ? ChallengeDataProvider.ChallengePermissionState.Normal : ChallengeDataProvider.ChallengePermissionState.Restricted_InMatch));
		}
	}

	private IEnumerator Coroutine_OpenPlayBlade_ViewFriendChallenge(Guid challengeId)
	{
		if (_sceneLoader != null && _sceneLoader.CurrentContentType != NavContentType.Home)
		{
			_sceneLoader.GoToLanding(new HomePageContext
			{
				InitialBladeState = PlayBladeController.PlayBladeVisualStates.Challenge
			});
			yield return new WaitUntil(() => _sceneLoader.IsLoading);
			yield return new WaitUntil(() => !_sceneLoader.IsLoading);
		}
		else
		{
			((_sceneLoader != null) ? (_sceneLoader.CurrentNavContent as HomePageContentController) : null)?.HidePlayblade();
		}
		yield return new WaitUntil(() => _sceneLoader != null && _sceneLoader.CurrentContentType == NavContentType.Home);
		PlayBladeController playBladeController = ((_sceneLoader != null) ? (_sceneLoader.CurrentNavContent as HomePageContentController) : null)?.ChallengeBladeController;
		PVPChallengeData challengeData = _challengeController.GetChallengeData(challengeId);
		if (playBladeController != null && playBladeController.PlayBladeVisualState != PlayBladeController.PlayBladeVisualStates.Hidden)
		{
			Minimize();
		}
		if (!(playBladeController != null) || challengeData == null || playBladeController.PlayBladeVisualState != PlayBladeController.PlayBladeVisualStates.Challenge || !(challengeData.ChallengeId == playBladeController.CurrentChallengeId))
		{
			AudioManager.PlayAudio("sfx_ui_friends_close", _animatorCornerIcon.gameObject);
			Minimize();
			playBladeController.ViewFriendChallenge(challengeData.ChallengeId);
		}
	}

	public void Animation_HideSocialWidget()
	{
		Minimize();
	}

	public static bool IsSendFriendInviteShowing()
	{
		if (_instance != null)
		{
			TimeSpan timeSpan = DateTime.UtcNow - _instance._timeInvitePanelClosed;
			if (!(_instance._friendInvitePanel != null) || !_instance._friendInvitePanel.gameObject.activeSelf)
			{
				return timeSpan < TimeSpan.FromSeconds(0.5);
			}
			return true;
		}
		return false;
	}

	public void Show()
	{
		if (!_disabled)
		{
			if (_chatManager.Conversations.Count > 0)
			{
				ShowChatWindow();
			}
			else if (!SocialEntitiesListVisible)
			{
				ShowSocialEntitiesList();
			}
		}
	}

	public bool HandleKeyUp(KeyCode curr, Modifiers mods)
	{
		if (_disabled)
		{
			return false;
		}
		if (curr == KeyCode.Escape && (SocialEntitiesListVisible || ChatVisible))
		{
			return true;
		}
		if (!ChatVisible)
		{
			return InvitePanelVisible;
		}
		return true;
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (_disabled)
		{
			return false;
		}
		if (curr == KeyCode.Escape)
		{
			if (ChatVisible)
			{
				CloseChat();
				return true;
			}
			if (SocialEntitiesListVisible)
			{
				if (_friendInvitePanel != null)
				{
					_friendInvitePanel.Close();
				}
				else
				{
					Minimize();
				}
				return true;
			}
		}
		if (!ChatVisible)
		{
			return InvitePanelVisible;
		}
		return true;
	}

	public virtual void OnNext()
	{
		if (!(_friendInvitePanel != null))
		{
			if (ChatVisible)
			{
				_chatManager.SelectNextConversation();
			}
			else if (_chatManager.Conversations.Count > 0)
			{
				ShowChatWindow();
			}
			else
			{
				Minimize();
			}
		}
	}

	public virtual void OnPrevious()
	{
		if (ChatVisible)
		{
			_chatManager.SelectNextConversation(reverse: true);
		}
		else
		{
			Minimize();
		}
	}

	public virtual void OnAccept()
	{
		if (ChatVisible)
		{
			_chatWindow.TrySendMessage();
		}
	}

	public void OnBack(ActionContext context)
	{
		if (ChatVisible)
		{
			CloseChat();
		}
		else if (SocialEntitiesListVisible)
		{
			if (_friendInvitePanel != null)
			{
				_friendInvitePanel.Close();
			}
			else
			{
				Minimize();
			}
		}
	}
}
