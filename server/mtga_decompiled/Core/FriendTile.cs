using System;
using Core.Code.Input;
using Core.Code.Promises;
using Core.Meta.MainNavigation.Challenge;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.Arena.Promises;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wizards.Mtga.PrivateGame;
using Wotc.Mtga.CustomInput;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class FriendTile : MonoBehaviour, IBackActionHandler
{
	private enum AnimatorStates
	{
		Offline,
		Available,
		Busy,
		Away
	}

	public enum Context
	{
		SocialEntityList,
		ChatWindow
	}

	[Header("Label References")]
	[SerializeField]
	private TMP_Text _labelName;

	[SerializeField]
	private Localize _labelStatus;

	[Header("Button References")]
	[SerializeField]
	private CustomButton _contextClickButton;

	[SerializeField]
	private CustomTouchButton _contextClickButtonHandheld;

	[SerializeField]
	private RectTransform _contextMenu;

	[SerializeField]
	private float _contextMenuBottomMargin;

	[SerializeField]
	private Button _buttonRemoveFriend;

	[SerializeField]
	private Button _buttonBlockFriend;

	[SerializeField]
	private Button _buttonChallengeFriend;

	[Header("Other")]
	[SerializeField]
	private TooltipTrigger _tooltipChallengeRestricted;

	private Animator _animator;

	public Action<SocialEntity> Callback_OpenChat;

	public Action<SocialEntity> Callback_RemoveFriend;

	public Action<SocialEntity> Callback_BlockFriend;

	private Context _context;

	private bool _isNewFriend;

	private bool _contextMenuActive;

	private bool _challengeEnabled = true;

	private RectTransform _dropdownRoot;

	private static readonly int FriendStatus = Animator.StringToHash("FriendStatus");

	private static readonly int ChallengeOut = Animator.StringToHash("ChallengeOUT");

	private static readonly int ChallengeIn = Animator.StringToHash("ChallengeIN");

	private static readonly int FriendHasChatHistory = Animator.StringToHash("HasChatHistory");

	private static readonly int FriendHasUnreadChats = Animator.StringToHash("HasUnreadChat");

	private static readonly int IsChatTile = Animator.StringToHash("ChatItem");

	private static readonly int ChallengeEnabled = Animator.StringToHash("ChallengeENABLED");

	private PVPChallengeController _challengeController;

	public Action<Guid> OpenPlayBlade;

	private IActionSystem _actionSystem;

	public SocialEntity Friend { get; private set; }

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		_contextClickButton.OnClick.AddListener(OnButton_OpenChat);
		_contextClickButton.OnRightClick.AddListener(OnContextClick);
		if (_contextClickButtonHandheld != null)
		{
			_contextClickButtonHandheld.OnClick.AddListener(OnButton_OpenChat);
			_contextClickButtonHandheld.OnClickAndHold.AddListener(OnContextClick);
		}
		_buttonRemoveFriend.onClick.AddListener(OnButton_RemoveFriend);
		_buttonBlockFriend.onClick.AddListener(OnButton_BlockFriend);
		_buttonChallengeFriend.onClick.AddListener(OnButton_ChallengeFriend);
		_challengeController = Pantry.Get<PVPChallengeController>();
	}

	private void OnEnable()
	{
		UpdateStatus();
	}

	public void Init(SocialEntity friend, Context context, IActionSystem actionSystem, ChallengeDataProvider.ChallengePermissionState challengePermission = ChallengeDataProvider.ChallengePermissionState.None, RectTransform dropdownRoot = null)
	{
		_actionSystem = actionSystem;
		Friend = friend;
		_context = context;
		_dropdownRoot = dropdownRoot;
		base.gameObject.UpdateActive(active: true);
		_labelName.text = Friend?.DisplayName ?? "";
		UpdateChallenge(challengePermission);
		UpdateStatus();
	}

	public void SetChallengeEnabled(bool value)
	{
		if (_challengeEnabled != value)
		{
			_challengeEnabled = value;
			UpdateStatus();
		}
	}

	private void OnButton_RemoveFriend()
	{
		AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
		Callback_RemoveFriend?.Invoke(Friend);
	}

	private void OnButton_BlockFriend()
	{
		AudioManager.PlayAudio("sfx_ui_friends_negate", base.gameObject);
		Callback_BlockFriend?.Invoke(Friend);
	}

	private void OnButton_OpenChat()
	{
		if (Friend.IsOnline || Friend.HasChatHistory)
		{
			SetContextMenuActive(isActive: false);
			AudioManager.PlayAudio("sfx_ui_friends_click", base.gameObject);
			Callback_OpenChat?.Invoke(Friend);
			UpdateStatus();
		}
	}

	private void OnButton_ChallengeFriend()
	{
		AudioManager.PlayAudio("sfx_ui_friends_send_invite", base.gameObject);
		PVPChallengeData activeCurrentChallengeData = _challengeController.GetActiveCurrentChallengeData();
		if (activeCurrentChallengeData == null)
		{
			_challengeController.CreateAndCacheChallenge().ThenOnMainThread(delegate(Promise<PVPChallengeData> promise)
			{
				if (promise.Successful)
				{
					AddInviteAndSend(promise.Result);
				}
			});
		}
		else
		{
			AddInviteAndSend(activeCurrentChallengeData);
		}
	}

	private void AddInviteAndSend(PVPChallengeData challenge)
	{
		_challengeController.AddChallengeInvite(challenge.ChallengeId, Friend.FullName, Friend.PlayerId);
		_challengeController.SendChallengeInvites(challenge.ChallengeId).ThenOnMainThreadIfSuccess(delegate(bool result)
		{
			if (result)
			{
				OpenPlayBlade?.Invoke(challenge.ChallengeId);
			}
		});
	}

	private void UpdateChallenge(ChallengeDataProvider.ChallengePermissionState permission)
	{
		if (Friend == null || !Friend.IsOnline)
		{
			_tooltipChallengeRestricted.IsActive = false;
			return;
		}
		bool flag = false;
		string text = null;
		PVPChallengeData activeCurrentChallengeData = _challengeController.GetActiveCurrentChallengeData();
		if (activeCurrentChallengeData == null)
		{
			switch (permission)
			{
			case ChallengeDataProvider.ChallengePermissionState.Restricted_InMatch:
				flag = true;
				text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Challenges/CurrentlyInMatch");
				break;
			case ChallengeDataProvider.ChallengePermissionState.Restricted_MaxOutgoingChallengesReached:
				flag = true;
				text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/FriendChallenge/CannotSendMoreThanOneChallenge_Tooltip");
				break;
			case ChallengeDataProvider.ChallengePermissionState.Restricted_ChallengesKillswitched:
				flag = true;
				text = Languages.ActiveLocProvider.GetLocalizedText("MainNav/Challenges/ChallengesKillswitched");
				break;
			}
		}
		else if (activeCurrentChallengeData.ChallengePlayers.ContainsKey(Friend.PlayerId) || activeCurrentChallengeData.Invites.ContainsKey(Friend.PlayerId) || activeCurrentChallengeData.IsChallengeFull)
		{
			flag = true;
		}
		_buttonChallengeFriend.interactable = !flag;
		_tooltipChallengeRestricted.IsActive = flag && !string.IsNullOrEmpty(text);
		_tooltipChallengeRestricted.TooltipData.Text = text;
	}

	public void UpdateStatus()
	{
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		PresenceStatus presenceStatus = Friend?.Status ?? PresenceStatus.Offline;
		switch (presenceStatus)
		{
		case PresenceStatus.Offline:
			_labelStatus.SetText("Social/Friends/ConnectionState/Offline");
			_animator.SetInteger(FriendStatus, 0);
			break;
		case PresenceStatus.Away:
			_labelStatus.SetText("Social/Friends/ConnectionState/Away");
			_animator.SetInteger(FriendStatus, 3);
			break;
		case PresenceStatus.Busy:
			_labelStatus.SetText("Social/Friends/ConnectionState/Busy");
			_animator.SetInteger(FriendStatus, 2);
			break;
		case PresenceStatus.Available:
			_labelStatus.SetText("Social/Friends/ConnectionState/Online");
			_animator.SetInteger(FriendStatus, 1);
			break;
		}
		if (Friend != null)
		{
			PVPChallengeData challengeData = _challengeController.GetChallengeData(Friend.PlayerId);
			if (PlatformUtils.TouchSupported() && PlatformUtils.IsHandheld())
			{
				_buttonChallengeFriend.gameObject.SetActive(_challengeEnabled && presenceStatus != PresenceStatus.Offline && challengeData == null);
			}
		}
		_animator.SetBool(ChallengeIn, value: false);
		_animator.SetBool(ChallengeOut, value: false);
		_animator.SetBool(FriendHasUnreadChats, Friend?.HasUnseenMessages ?? false);
		_animator.SetBool(FriendHasChatHistory, Friend?.HasChatHistory ?? false);
		_animator.SetBool(IsChatTile, _context == Context.ChatWindow);
		_animator.SetBool(ChallengeEnabled, _challengeEnabled);
	}

	private void OnDisable()
	{
		SetContextMenuActive(isActive: false);
	}

	public void Cleanup()
	{
		OpenPlayBlade = null;
		SetContextMenuActive(isActive: false);
		Friend = null;
		Callback_OpenChat = null;
		Callback_RemoveFriend = null;
		Callback_BlockFriend = null;
	}

	private void OnContextClick()
	{
		SetContextMenuActive(!_contextMenuActive);
	}

	private void Update()
	{
		if (!_contextMenuActive)
		{
			return;
		}
		if (CustomInputModule.PointerIsHeldDown() || CustomInputModule.IsRightClick())
		{
			if (!((RectTransform)base.transform).GetMouseOver() && !((RectTransform)_contextMenu.transform).GetMouseOver())
			{
				SetContextMenuActive(isActive: false);
			}
		}
		else if (!ActionSystemFactory.CaptureEscapeFeatureToggle && Input.GetKeyDown(KeyCode.Escape))
		{
			SetContextMenuActive(isActive: false);
		}
		else if (CustomInputModule.GetMouseScroll() != Vector2.zero)
		{
			SetContextMenuActive(isActive: false);
		}
	}

	private void SetContextMenuActive(bool isActive)
	{
		if (_contextMenuActive == isActive || _contextMenu == null)
		{
			return;
		}
		AudioManager.PlayAudio("sfx_ui_friends_click", base.gameObject);
		_contextMenuActive = isActive;
		_contextMenu.gameObject.UpdateActive(_contextMenuActive);
		if (_contextMenuActive)
		{
			_actionSystem?.PushFocus(this);
			Transform parent = (isActive ? _dropdownRoot : base.transform);
			_contextMenu.SetParent(parent, worldPositionStays: true);
			Vector2 anchoredPosition = _contextMenu.anchoredPosition;
			if (anchoredPosition.y - _contextMenu.rect.height < _contextMenuBottomMargin)
			{
				anchoredPosition.y -= anchoredPosition.y - _contextMenu.rect.height - _contextMenuBottomMargin;
				_contextMenu.anchoredPosition = anchoredPosition;
			}
		}
		else
		{
			_contextMenu.SetParent(base.transform);
			_contextMenu.anchoredPosition = new Vector2(0f, 0f);
			_actionSystem?.PopFocus(this);
		}
	}

	public void OnBack(ActionContext context)
	{
		if (ActionSystemFactory.CaptureEscapeFeatureToggle)
		{
			SetContextMenuActive(isActive: false);
			context.Used = false;
		}
	}
}
