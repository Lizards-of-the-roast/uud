using System.Collections.Generic;
using HasbroGo.Accounts.Models;
using HasbroGo.Helpers;
using HasbroGo.Logging;
using HasbroGo.Social.Models;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HasbroGo;

public class FriendTile : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerEnterHandler, IPointerExitHandler
{
	public delegate void BlockUnblockPanelWillShow();

	public delegate void OnOpenChat(Friend friend);

	public delegate void OnChallenge(string displayName);

	[SerializeField]
	private TextMeshProUGUI displayName;

	[SerializeField]
	private TextMeshProUGUI friendStatus;

	[SerializeField]
	private Image friendPresenceIcon;

	[SerializeField]
	private Sprite offlinePresenceSprite;

	[SerializeField]
	private Sprite onlinePresenceSprite;

	[SerializeField]
	private Button onlineFriendChatButton;

	[SerializeField]
	private Button onlineFriendChallengeButton;

	[SerializeField]
	private Button outgoingRequestRevokeButton;

	[SerializeField]
	private Button blockedUserUnfriendButton;

	[SerializeField]
	private GameObject unfriendAndBlockPanel;

	[SerializeField]
	private GameObject unfriendAndUnblockPanel;

	[SerializeField]
	private GameObject declineInviteAndBlockPanel;

	[SerializeField]
	private GameObject actualFriendTileView;

	[SerializeField]
	private GameObject incomingRequestTileView;

	[SerializeField]
	private GameObject outgoingRequestTileView;

	[SerializeField]
	private GameObject blockedFriendTileView;

	private readonly Dictionary<FriendTileType, GameObject> friendTileTypeViewMap = new Dictionary<FriendTileType, GameObject>();

	private bool _hasUnreadMessage;

	private bool _hasUpdatedChatButton;

	private readonly string _logCategory = "FriendTile";

	public FriendWithPresence FriendWithPresence { get; private set; }

	public BlockedUser BlockedUser { get; private set; }

	public IncomingFriendInvite IncomingFriendInvite { get; private set; }

	public OutgoingFriendInvite OutgoingFriendInvite { get; private set; }

	public FriendTileType FriendTileType { get; private set; }

	public event BlockUnblockPanelWillShow BlockUnblockPanelWillShowEvent;

	public event OnOpenChat OnOpenChatEvent;

	public event OnChallenge OnChallengeEvent;

	public void Init(FriendWithPresence friendWithPresence, BlockedUser blockedUser, IncomingFriendInvite incomingFriendInvite, OutgoingFriendInvite outgoingFriendInvite, FriendTileType friendTileType)
	{
		friendTileTypeViewMap.Add(FriendTileType.ActualFriend, actualFriendTileView);
		friendTileTypeViewMap.Add(FriendTileType.IncomingInvite, incomingRequestTileView);
		friendTileTypeViewMap.Add(FriendTileType.OutgoingInvite, outgoingRequestTileView);
		friendTileTypeViewMap.Add(FriendTileType.BlockedFriend, blockedFriendTileView);
		FriendWithPresence = friendWithPresence;
		BlockedUser = blockedUser;
		IncomingFriendInvite = incomingFriendInvite;
		OutgoingFriendInvite = outgoingFriendInvite;
		FriendTileType = friendTileType;
		UpdateDisplayName();
		UpdateFriendStatus();
		ShowFriendTileTypeView();
	}

	private void OnDestroy()
	{
		onlineFriendChatButton.onClick.RemoveListener(OnOpenChatButton);
		onlineFriendChallengeButton.onClick.RemoveListener(OnChallengeButton);
	}

	private void Update()
	{
		if (_hasUnreadMessage && !_hasUpdatedChatButton)
		{
			_hasUpdatedChatButton = true;
			ColorBlock colors = onlineFriendChatButton.colors;
			colors.normalColor = Color.yellow;
			onlineFriendChatButton.colors = colors;
			onlineFriendChatButton.gameObject.SetActive(value: true);
		}
	}

	public void HideBlockUnblockPanels()
	{
		ToggleFriendActionPanel(unfriendAndBlockPanel, forceHide: true);
		ToggleFriendActionPanel(unfriendAndUnblockPanel, forceHide: true);
		ToggleFriendActionPanel(declineInviteAndBlockPanel, forceHide: true);
	}

	private void UpdateDisplayName()
	{
		if (BlockedUser != null)
		{
			displayName.text = BlockedUser.DisplayName.ToString();
		}
		else if (FriendWithPresence != null)
		{
			string text = FriendWithPresence.Friend?.DisplayName?.ToString();
			if (!string.IsNullOrEmpty(text))
			{
				displayName.text = text;
			}
			else
			{
				displayName.text = FriendWithPresence.Friend.AccountId;
			}
		}
		else if (IncomingFriendInvite != null)
		{
			displayName.text = IncomingFriendInvite.DisplayName.ToString();
		}
		else if (OutgoingFriendInvite != null)
		{
			DisplayName receiverDisplayName = OutgoingFriendInvite.ReceiverDisplayName;
			if (receiverDisplayName != null && receiverDisplayName.IsValid)
			{
				displayName.text = OutgoingFriendInvite.ReceiverDisplayName.ToString();
			}
			else
			{
				Email receiverEmail = OutgoingFriendInvite.ReceiverEmail;
				if (receiverEmail != null && receiverEmail.IsValid)
				{
					displayName.text = OutgoingFriendInvite.ReceiverEmail.ToString();
				}
				else if (!string.IsNullOrEmpty(OutgoingFriendInvite.ReceiverAccountId))
				{
					displayName.text = OutgoingFriendInvite.ReceiverAccountId;
				}
				else if (!string.IsNullOrEmpty(OutgoingFriendInvite.ReceiverPersonaId))
				{
					displayName.text = OutgoingFriendInvite.ReceiverPersonaId;
				}
				else
				{
					LogHelper.Logger.Log(_logCategory, LogLevel.Error, "Outgoing friend invite " + OutgoingFriendInvite.InviteId + " was invited by an unsupported method. Current supported invite methods are by display name or email.");
					displayName.text = "Unknown Account";
				}
			}
		}
		if (FriendWithPresence == null)
		{
			displayName.color = friendStatus.color;
		}
	}

	private void UpdateFriendStatus()
	{
		switch (FriendTileType)
		{
		case FriendTileType.ActualFriend:
		{
			bool flag = IsFriendOnline();
			friendStatus.text = (flag ? "Online" : "Offline");
			friendPresenceIcon.sprite = (flag ? onlinePresenceSprite : offlinePresenceSprite);
			if (flag)
			{
				onlineFriendChatButton.onClick.AddListener(OnOpenChatButton);
				onlineFriendChallengeButton.onClick.AddListener(OnChallengeButton);
			}
			else
			{
				onlineFriendChatButton.onClick.RemoveListener(OnOpenChatButton);
				onlineFriendChallengeButton.onClick.RemoveListener(OnChallengeButton);
			}
			break;
		}
		case FriendTileType.IncomingInvite:
			friendStatus.text = "Pending Request";
			break;
		case FriendTileType.OutgoingInvite:
			friendStatus.text = "Request sent: " + OutgoingFriendInvite.CreatedAtUtc.ToLocalTime().ToString("M/dd/yyyy");
			break;
		case FriendTileType.BlockedFriend:
			friendStatus.text = "Blocked";
			break;
		}
	}

	private void ShowFriendTileTypeView()
	{
		foreach (GameObject value in friendTileTypeViewMap.Values)
		{
			value.SetActive(value: false);
		}
		friendTileTypeViewMap[FriendTileType].SetActive(value: true);
	}

	private void ToggleFriendActionPanel(GameObject panel, bool forceHide = false)
	{
		bool flag = !panel.activeInHierarchy && !forceHide;
		if (flag)
		{
			this.BlockUnblockPanelWillShowEvent();
		}
		panel.SetActive(flag);
	}

	private bool IsFriendOnline()
	{
		if (FriendWithPresence != null && FriendWithPresence.Presence != null)
		{
			return FriendWithPresence.Presence.PlatformStatus != PlatformStatus.Offline;
		}
		return false;
	}

	public void UpdateUnreadMessage()
	{
		_hasUnreadMessage = true;
	}

	public void AcceptIncomingRequestButtonClicked()
	{
		if (!string.IsNullOrEmpty(IncomingFriendInvite?.InviteId))
		{
			SocialManager.Instance.AcceptIncomingFriendInvite(IncomingFriendInvite.InviteId);
		}
		else
		{
			LogHelper.Logger.Log(_logCategory, LogLevel.Error, "Attempting to accept incoming invite without a valid invite id.");
		}
	}

	public void DeclineIncomingRequestButtonClicked()
	{
		if (!string.IsNullOrEmpty(IncomingFriendInvite?.InviteId))
		{
			SocialManager.Instance.DeclineIncomingFriendInvite(IncomingFriendInvite.InviteId);
		}
		else
		{
			LogHelper.Logger.Log(_logCategory, LogLevel.Error, "Attempting to decline incoming invite without a valid invite id.");
		}
	}

	public void RevokeOutgoingRequestButtonClicked()
	{
		if (!string.IsNullOrEmpty(OutgoingFriendInvite?.InviteId))
		{
			SocialManager.Instance.RevokeOutgoingFriendInvite(OutgoingFriendInvite.InviteId);
		}
		else
		{
			LogHelper.Logger.Log(_logCategory, LogLevel.Error, "Attempting to revoke outgoing invite without a valid invite id.");
		}
	}

	public void UnfriendButtonClicked()
	{
		ToggleFriendActionPanel(unfriendAndUnblockPanel, forceHide: true);
		ToggleFriendActionPanel(unfriendAndBlockPanel, forceHide: true);
		if (FriendWithPresence != null)
		{
			SocialManager.Instance.Unfriend(FriendWithPresence.Friend);
		}
		else
		{
			LogHelper.Logger.Log(_logCategory, LogLevel.Error, "Attempting to Unfriend a user with No FriendWithPresence or BlockedUser.");
		}
	}

	public void BlockFriendButtonClicked()
	{
		ToggleFriendActionPanel(unfriendAndBlockPanel, forceHide: true);
		if (FriendWithPresence != null)
		{
			SocialManager.Instance.BlockFriend(FriendWithPresence.Friend.AccountId);
		}
		else if (BlockedUser != null)
		{
			LogHelper.Logger.Log(_logCategory, LogLevel.Error, "Attempting to block user that is set as blocked. Account ID: " + BlockedUser.AccountId);
		}
		else
		{
			LogHelper.Logger.Log(_logCategory, LogLevel.Error, "Attempting to block user with unknown account id.");
		}
	}

	public void UnblockFriendButtonClicked()
	{
		ToggleFriendActionPanel(unfriendAndUnblockPanel, forceHide: true);
		if (BlockedUser != null)
		{
			SocialManager.Instance.UnblockFriend(BlockedUser);
		}
		else if (FriendWithPresence != null)
		{
			LogHelper.Logger.Log(_logCategory, LogLevel.Error, "Attempting to unblock user that is not set as blocked. Account ID: " + FriendWithPresence.Friend.AccountId);
		}
		else
		{
			LogHelper.Logger.Log(_logCategory, LogLevel.Error, "Attempting to unblock user with unknown account id.");
		}
	}

	public void DeclineInviteAndBlockUserButtonClicked()
	{
		ToggleFriendActionPanel(declineInviteAndBlockPanel, forceHide: true);
		if (IncomingFriendInvite != null)
		{
			SocialManager.Instance.DeclineIncomingInviteAndBlockUser(IncomingFriendInvite.AccountId, IncomingFriendInvite.InviteId);
		}
		else
		{
			LogHelper.Logger.Log(_logCategory, LogLevel.Error, "Attempting to decline a friend invite and block the sender with an invalid IncomingFriendInvite.");
		}
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Right)
		{
			if (FriendTileType == FriendTileType.ActualFriend)
			{
				ToggleFriendActionPanel(unfriendAndBlockPanel);
			}
			else if (FriendTileType == FriendTileType.BlockedFriend)
			{
				ToggleFriendActionPanel(unfriendAndUnblockPanel);
			}
			else if (FriendTileType == FriendTileType.IncomingInvite)
			{
				ToggleFriendActionPanel(declineInviteAndBlockPanel);
			}
		}
	}

	public void OnPointerEnter(PointerEventData data)
	{
		if (IsFriendOnline())
		{
			friendPresenceIcon.gameObject.SetActive(value: false);
			onlineFriendChatButton.gameObject.SetActive(value: true);
			onlineFriendChallengeButton.gameObject.SetActive(value: true);
		}
		else if (OutgoingFriendInvite != null)
		{
			outgoingRequestRevokeButton.gameObject.SetActive(value: true);
		}
		else if (BlockedUser != null)
		{
			blockedUserUnfriendButton.gameObject.SetActive(value: true);
		}
	}

	public void OnPointerExit(PointerEventData data)
	{
		if (FriendWithPresence != null)
		{
			friendPresenceIcon.gameObject.SetActive(value: true);
			onlineFriendChallengeButton.gameObject.SetActive(value: false);
			if (!_hasUnreadMessage)
			{
				onlineFriendChatButton.gameObject.SetActive(value: false);
			}
		}
		else if (OutgoingFriendInvite != null)
		{
			outgoingRequestRevokeButton.gameObject.SetActive(value: false);
		}
		else if (BlockedUser != null)
		{
			blockedUserUnfriendButton.gameObject.SetActive(value: false);
		}
	}

	public void OnOpenChatButton()
	{
		if (_hasUnreadMessage)
		{
			ColorBlock colors = onlineFriendChatButton.colors;
			colors.normalColor = Color.white;
			onlineFriendChatButton.colors = colors;
			_hasUnreadMessage = false;
			_hasUpdatedChatButton = false;
		}
		this.OnOpenChatEvent?.Invoke(FriendWithPresence.Friend);
	}

	public void OnChallengeButton()
	{
		this.OnChallengeEvent?.Invoke(displayName.text);
	}
}
