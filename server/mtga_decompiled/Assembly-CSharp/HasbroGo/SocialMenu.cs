using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HasbroGo.Accounts.Events;
using HasbroGo.Accounts.Profile;
using HasbroGo.Errors;
using HasbroGo.Helpers;
using HasbroGo.Logging;
using HasbroGo.Social.Events;
using HasbroGo.Social.Models;
using HasbroGo.Social.Models.Requests;
using Newtonsoft.Json;
using UnityEngine;

namespace HasbroGo;

public class SocialMenu : MonoBehaviour
{
	[SerializeField]
	private SocialBust socialBust;

	[SerializeField]
	private ChangeSocialPresencePanel changePresencePanel;

	[SerializeField]
	private SocialPanel socialPanel;

	[SerializeField]
	private SendFriendRequestPanel sendFriendRequestPanel;

	[SerializeField]
	private GameObject closeSocialPanelButtonGO;

	[SerializeField]
	private GameObject friendListContent;

	[SerializeField]
	private FriendTile friendTilePrefab;

	[SerializeField]
	private FriendTileHeader friendTileHeaderPrefab;

	private List<FriendTile> friendTiles = new List<FriendTile>();

	private List<FriendTileHeader> friendTileHeaders = new List<FriendTileHeader>();

	[SerializeField]
	private FriendChat friendChat;

	[SerializeField]
	private ChallengeMenu challengeMenu;

	private bool isChallengeMenuShowing;

	private bool isFriendChatShowing;

	private bool isFriendListBeingPopulated;

	private bool shouldRepopulateFriendList;

	private readonly string logCategory = "SocialMenu";

	private HasbroGoSDK sdk => HasbroGoSDKManager.Instance.HasbroGoSdk;

	public void OnEnable()
	{
		if (!IsSDKUseable())
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Log, "Disabling social menu because the HasbroGo SDK is not in a useable state. Ensure that it has been created and has both the AuthManager and AccountsService attached. ");
			base.gameObject.SetActive(value: false);
			return;
		}
		ResetViews();
		if (sdk.AccountsService.IsLoggedIn())
		{
			UpdateUserDisplayName();
			UpdateUserPresence();
			PopulateFriendList();
		}
		sdk.AccountsService.LoginSuccessful += OnUserLoggedIn;
		SocialManager.Instance.FriendBlockedEvent += OnFriendActionSucceeded;
		SocialManager.Instance.FriendUnblockedEvent += OnFriendActionSucceeded;
		SocialManager.Instance.FriendInviteSentEvent += OnFriendActionSucceeded;
		SocialManager.Instance.FriendIncomingInviteAcceptedEvent += OnFriendActionSucceeded;
		SocialManager.Instance.FriendIncomingInviteDeclinedEvent += OnFriendActionSucceeded;
		SocialManager.Instance.FriendIncomingInviteDeclinedAndUserBlockedEvent += OnFriendActionSucceeded;
		SocialManager.Instance.FriendOutgoingInviteRevokedEvent += OnFriendActionSucceeded;
		SocialManager.Instance.FriendActionErrorEvent += OnSocialManagerActionError;
		SocialManager.Instance.FriendUnfriendedEvent += OnFriendActionSucceeded;
		SocialManager.Instance.PresenceUpdatedEvent += OnUserPresenceUpdateSucceeded;
		SocialManager.Instance.PresenceActionErrorEvent += OnSocialManagerActionError;
		SocialManager.Instance.ChatMessageReceivedEvent += OnChatMessageReceived;
		sdk.SocialService.OnIncomingFriendInviteReceived += OnIncomingFriendInviteReceived;
		sdk.SocialService.OnFriendAdded += OnFriendAdded;
		sdk.SocialService.OnFriendRemoved += OnFriendRemoved;
		sdk.SocialService.OnIncomingFriendInviteRevoked += OnFriendInviteRevoked;
		sdk.SocialService.OnFriendInviteRemoved += OnFriendInviteRemoved;
		sdk.SocialService.OnFriendPresenceUpdateReceived += OnFriendPresenceUpdateReceived;
		SocialManager.Instance.ChallengeReceivedEvent += OnChallengeMessageReceived;
	}

	public void OnDisable()
	{
		if (IsSDKUseable())
		{
			sdk.AccountsService.LoginSuccessful -= OnUserLoggedIn;
		}
		SocialManager.Instance.FriendBlockedEvent -= OnFriendActionSucceeded;
		SocialManager.Instance.FriendUnblockedEvent -= OnFriendActionSucceeded;
		SocialManager.Instance.FriendInviteSentEvent -= OnFriendActionSucceeded;
		SocialManager.Instance.FriendIncomingInviteAcceptedEvent -= OnFriendActionSucceeded;
		SocialManager.Instance.FriendIncomingInviteDeclinedEvent -= OnFriendActionSucceeded;
		SocialManager.Instance.FriendIncomingInviteDeclinedAndUserBlockedEvent -= OnFriendActionSucceeded;
		SocialManager.Instance.FriendOutgoingInviteRevokedEvent -= OnFriendActionSucceeded;
		SocialManager.Instance.FriendActionErrorEvent -= OnSocialManagerActionError;
		SocialManager.Instance.FriendUnfriendedEvent -= OnFriendActionSucceeded;
		SocialManager.Instance.PresenceUpdatedEvent -= OnUserPresenceUpdateSucceeded;
		SocialManager.Instance.PresenceActionErrorEvent -= OnSocialManagerActionError;
		SocialManager.Instance.ChatMessageReceivedEvent -= OnChatMessageReceived;
		sdk.SocialService.OnIncomingFriendInviteReceived -= OnIncomingFriendInviteReceived;
		sdk.SocialService.OnFriendAdded -= OnFriendAdded;
		sdk.SocialService.OnFriendRemoved -= OnFriendRemoved;
		sdk.SocialService.OnIncomingFriendInviteRevoked -= OnFriendInviteRevoked;
		sdk.SocialService.OnFriendInviteRemoved -= OnFriendInviteRemoved;
		sdk.SocialService.OnFriendPresenceUpdateReceived -= OnFriendPresenceUpdateReceived;
		SocialManager.Instance.ChallengeReceivedEvent -= OnChallengeMessageReceived;
	}

	public void LateUpdate()
	{
		if (shouldRepopulateFriendList && !isFriendListBeingPopulated)
		{
			shouldRepopulateFriendList = false;
			PopulateFriendList();
		}
	}

	public void OpenSocialPanelButtonClicked()
	{
		if (sdk.AccountsService.IsLoggedIn())
		{
			ShowSocialPanel(show: true);
		}
		else
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Warning, "User must be logged in before accessing social features.");
		}
	}

	public void CloseSocialPanelButtonClicked()
	{
		ShowSocialPanel(show: false);
	}

	public void OpenSendFriendRequestPanelButtonClicked()
	{
		sendFriendRequestPanel.gameObject.SetActive(value: true);
	}

	public void CloseSendFriendRequestPanelButtonClicked()
	{
		sendFriendRequestPanel.gameObject.SetActive(value: false);
	}

	public void ToggleChangePresencePanelButtonClicked()
	{
		changePresencePanel.gameObject.SetActive(!changePresencePanel.gameObject.activeInHierarchy);
	}

	public void OpenChallengeButtonClicked()
	{
		if (!isChallengeMenuShowing)
		{
			challengeMenu.gameObject.SetActive(value: true);
			challengeMenu.Init(string.Empty);
			isChallengeMenuShowing = true;
		}
		else
		{
			isChallengeMenuShowing = false;
			challengeMenu.gameObject.SetActive(value: false);
		}
	}

	private Task OnUserLoggedIn(object sender, LoginSuccessfulEvent loginSuccessfulEvent, CancellationToken token)
	{
		UpdateUserDisplayName();
		UpdateUserPresence();
		PopulateFriendList();
		return Task.CompletedTask;
	}

	private void OnFriendActionSucceeded(object sender, EventArgs eventArgs)
	{
		PopulateFriendList();
	}

	private void OnUserPresenceUpdateSucceeded(object sender, EventArgs eventArgs)
	{
		UpdatePresenceEventArgs e = eventArgs as UpdatePresenceEventArgs;
		LogHelper.Logger.Log(logCategory, LogLevel.Log, "User presence updated. New Presence: " + e.Presence.PlatformStatus);
		UpdateUserPresence();
	}

	private void OnSocialManagerActionError(object sender, EventArgs eventArgs)
	{
		ErrorEventArgs e = eventArgs as ErrorEventArgs;
		if (e != null)
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Account action error occurred. Category: " + e.ErrorCategory.ToString() + ". Error: " + e.Error?.Message + ". Extra Data: " + e.ExtraData.ToString());
		}
		else
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, $"Invalid EventArgs type passed. Expected type: ErrorEventArgs. Actual type: {typeof(EventArgs)}");
		}
		if (e.ErrorCategory is SocialManager.FriendAction && (SocialManager.FriendAction)e.ErrorCategory == SocialManager.FriendAction.DeclineIncomingFriendInviteAndBlockUser)
		{
			PopulateFriendList();
		}
	}

	private void OnIncomingFriendInviteReceived(object sender, IncomingFriendInviteReceivedEventArgs e)
	{
		shouldRepopulateFriendList = true;
	}

	private void OnFriendAdded(object sender, FriendAddedEventArgs e)
	{
		shouldRepopulateFriendList = true;
	}

	private void OnFriendRemoved(object sender, FriendRemovedEventArgs e)
	{
		shouldRepopulateFriendList = true;
	}

	private void OnFriendInviteRevoked(object sender, FriendInviteRevokedEventArgs e)
	{
		shouldRepopulateFriendList = true;
	}

	private void OnFriendInviteRemoved(object sender, FriendInviteRemovedEventArgs e)
	{
		shouldRepopulateFriendList = true;
	}

	private void OnFriendPresenceUpdateReceived(object sender, PresenceUpdateReceivedEventArgs e)
	{
		shouldRepopulateFriendList = true;
	}

	private void ResetViews()
	{
		socialBust.ResetView();
		socialPanel.gameObject.SetActive(value: false);
		closeSocialPanelButtonGO.SetActive(value: false);
		changePresencePanel.gameObject.SetActive(value: false);
		sendFriendRequestPanel.gameObject.SetActive(value: false);
		friendChat.gameObject.SetActive(value: false);
		challengeMenu.gameObject.SetActive(value: false);
		isFriendChatShowing = false;
		isChallengeMenuShowing = false;
		HideAllFriendTileBlockUnblockPanels();
	}

	private void ShowSocialPanel(bool show)
	{
		socialBust.ShowNameAndOnlineFriendCountContainer(!show);
		socialPanel.gameObject.SetActive(show);
		closeSocialPanelButtonGO.SetActive(show);
		changePresencePanel.gameObject.SetActive(value: false);
		sendFriendRequestPanel.gameObject.SetActive(value: false);
		friendChat.gameObject.SetActive(value: false);
		challengeMenu.gameObject.SetActive(value: false);
		isChallengeMenuShowing = false;
		isFriendChatShowing = false;
		HideAllFriendTileBlockUnblockPanels();
	}

	private async void UpdateUserDisplayName()
	{
		Result<ProfileResponse, Error> result = await sdk.AccountsService.GetProfile();
		if (!result.IsOk)
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Error retrieving user profile.");
			return;
		}
		socialBust.UpdateDisplayName(result.Value.DisplayName);
		socialPanel.UpdateDisplayName(result.Value.DisplayName);
		challengeMenu.UpdateDisplayName(result.Value.DisplayName);
	}

	private bool IsSDKUseable()
	{
		if (sdk != null && sdk.AccountsService != null && sdk.SocialService != null)
		{
			return sdk.AuthManager != null;
		}
		return false;
	}

	private async void PopulateFriendList()
	{
		if (isFriendListBeingPopulated)
		{
			shouldRepopulateFriendList = true;
			return;
		}
		isFriendListBeingPopulated = true;
		CleanupFriendTiles();
		Result<IEnumerable<FriendWithPresence>, Error> result = await sdk.SocialService.GetFriendsWithPresence();
		if (result.IsOk)
		{
			int num = result.Value.Count();
			int num2 = 0;
			if (num > 0)
			{
				FriendTileHeader friendHeaderTile = AddFriendTileHeader($"Friends {num}");
				foreach (FriendWithPresence item in result.Value)
				{
					AddFriendTile(friendHeaderTile, item, null, null, null, FriendTileType.ActualFriend);
					if (item.Presence != null && item.Presence.PlatformStatus != PlatformStatus.Offline)
					{
						num2++;
					}
				}
			}
			socialBust.UpdateOnlineFriendsCount(num2);
		}
		else
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Error retrieving actual friends: " + result.Error.Message);
			socialBust.UpdateOnlineFriendsCount(0);
		}
		Result<IEnumerable<IncomingFriendInvite>, Error> result2 = await sdk.SocialService.GetIncomingFriendInvites();
		if (result2.IsOk)
		{
			int num3 = result2.Value.Count();
			if (num3 > 0)
			{
				FriendTileHeader friendHeaderTile2 = AddFriendTileHeader($"Incoming Requests {num3}");
				foreach (IncomingFriendInvite item2 in result2.Value)
				{
					AddFriendTile(friendHeaderTile2, null, null, item2, null, FriendTileType.IncomingInvite);
				}
			}
		}
		else
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Error retrieving incoming friend invites.");
		}
		Result<IEnumerable<OutgoingFriendInvite>, Error> result3 = await sdk.SocialService.GetOutgoingFriendInvites();
		if (result3.IsOk)
		{
			int num4 = result3.Value.Count();
			if (num4 > 0)
			{
				FriendTileHeader friendHeaderTile3 = AddFriendTileHeader($"Sent Requests {num4}");
				foreach (OutgoingFriendInvite item3 in result3.Value)
				{
					AddFriendTile(friendHeaderTile3, null, null, null, item3, FriendTileType.OutgoingInvite);
				}
			}
		}
		else
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Error retrieving outgoing friend invites.");
		}
		Result<IEnumerable<BlockedUser>, Error> result4 = await sdk.SocialService.GetBlockedUsers(new GetBlockListRequest(100));
		if (result4.IsOk)
		{
			if (result4.Value.Count() > 0)
			{
				FriendTileHeader friendHeaderTile4 = AddFriendTileHeader("Blocked Users");
				foreach (BlockedUser item4 in result4.Value)
				{
					AddFriendTile(friendHeaderTile4, null, item4, null, null, FriendTileType.BlockedFriend);
				}
			}
		}
		else
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Error retrieving blocked friends.");
		}
		isFriendListBeingPopulated = false;
		if (shouldRepopulateFriendList)
		{
			shouldRepopulateFriendList = false;
			PopulateFriendList();
		}
	}

	private FriendTileHeader AddFriendTileHeader(string headerTitle)
	{
		FriendTileHeader friendTileHeader = UnityEngine.Object.Instantiate(friendTileHeaderPrefab, friendListContent.transform, worldPositionStays: true);
		friendTileHeader.Init(headerTitle);
		friendTileHeaders.Add(friendTileHeader);
		return friendTileHeader;
	}

	private void AddFriendTile(FriendTileHeader friendHeaderTile, FriendWithPresence friendWithPresence, BlockedUser blockedUser, IncomingFriendInvite incomingFriendInvite, OutgoingFriendInvite outgoingFriendInvite, FriendTileType friendTileType)
	{
		FriendTile friendTile = UnityEngine.Object.Instantiate(friendTilePrefab, friendListContent.transform, worldPositionStays: true);
		friendTile.BlockUnblockPanelWillShowEvent += HideAllFriendTileBlockUnblockPanels;
		friendTile.Init(friendWithPresence, blockedUser, incomingFriendInvite, outgoingFriendInvite, friendTileType);
		friendTiles.Add(friendTile);
		if (friendWithPresence != null)
		{
			friendTile.OnOpenChatEvent += OpenChatMenu;
			friendTile.OnChallengeEvent += OpenChallengeMenu;
		}
		friendHeaderTile.AddFriendTile(friendTile);
	}

	private void OpenChatMenu(Friend friend)
	{
		friendChat.InitChat(friend);
		friendChat.gameObject.SetActive(value: true);
		isFriendChatShowing = true;
	}

	private void OpenChallengeMenu(string displayName)
	{
		challengeMenu.Init(displayName);
		challengeMenu.gameObject.SetActive(value: true);
		isChallengeMenuShowing = true;
	}

	private void OnChallengeMessageReceived(object sender, EventArgs e)
	{
		GameMessageReceivedEventArgs e2 = e as GameMessageReceivedEventArgs;
		if (isChallengeMenuShowing)
		{
			if (e2.GameMessage.SchemaId == "ChallengeSendData")
			{
				ChallengeSendData message = JsonConvert.DeserializeObject<ChallengeSendData>(e2.GameMessage.Payload);
				challengeMenu.HandleChallengeSendData(message);
			}
			else if (e2.GameMessage.SchemaId == "ChallengeConfirmData")
			{
				ChallengeConfirmData message2 = JsonConvert.DeserializeObject<ChallengeConfirmData>(e2.GameMessage.Payload);
				challengeMenu.HandleChallengeConfirmData(message2);
			}
		}
	}

	private void OnChatMessageReceived(object sender, EventArgs e)
	{
		DirectMessageReceivedEventArgs e2 = e as DirectMessageReceivedEventArgs;
		if (isFriendChatShowing && e2.Message.SenderAccountId == friendChat.Friend.AccountId)
		{
			friendChat.QueueMessage(e2);
			return;
		}
		foreach (FriendTile friendTile in friendTiles)
		{
			if (friendTile.FriendWithPresence.Friend.AccountId == e2.Message.SenderAccountId)
			{
				friendTile.UpdateUnreadMessage();
				break;
			}
		}
	}

	private void HideAllFriendTileBlockUnblockPanels()
	{
		foreach (FriendTile friendTile in friendTiles)
		{
			friendTile.HideBlockUnblockPanels();
		}
	}

	private void CleanupFriendTiles()
	{
		foreach (FriendTileHeader friendTileHeader in friendTileHeaders)
		{
			UnityEngine.Object.Destroy(friendTileHeader.gameObject);
		}
		friendTileHeaders.Clear();
		foreach (FriendTile friendTile in friendTiles)
		{
			friendTile.BlockUnblockPanelWillShowEvent -= HideAllFriendTileBlockUnblockPanels;
			UnityEngine.Object.Destroy(friendTile.gameObject);
		}
		friendTiles.Clear();
	}

	private async void UpdateUserPresence()
	{
		Result<Presence, Error> result = await sdk.SocialService.GetPresence();
		if (result.IsOk)
		{
			if (result.Value != null)
			{
				PlatformStatus platformStatus = result.Value.PlatformStatus;
				socialBust.UpdatePresenceIcon(platformStatus);
				socialPanel.UpdatePresenceStatus(platformStatus);
				if (platformStatus == PlatformStatus.Offline)
				{
					friendListContent.SetActive(value: false);
					changePresencePanel.ShowBusyPresenceButton(show: false);
					socialBust.ShowOnlineFriendCount(show: false);
					ShowSocialPanel(show: false);
				}
				else
				{
					friendListContent.SetActive(value: true);
					changePresencePanel.ShowBusyPresenceButton(show: true);
					socialBust.ShowOnlineFriendCount(show: true);
					changePresencePanel.gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			LogHelper.Logger.Log(logCategory, LogLevel.Error, "Failed to user's Presence with error: " + result.Error.Message);
		}
	}
}
