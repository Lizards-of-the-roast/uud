using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree;
using AssetLookupTree.Payloads.Prefab;
using Core.BI;
using Core.Code.AssetLookupTree.AssetLookup;
using Core.Meta.MainNavigation.Challenge;
using Core.Meta.MainNavigation.PopoutMenu.Challenges;
using Core.Meta.MainNavigation.SystemMessage;
using Core.Shared.Code.PVPChallenge;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using UnityEngine;
using Wizards.Mtga;
using Wizards.Mtga.PrivateGame;
using Wizards.Mtga.UnifiedChallenges;
using Wotc.Mtga;
using Wotc.Mtga.Loc;

public class UnifiedChallengeDisplay : MonoBehaviour
{
	[SerializeField]
	private CustomButton _leaveButton;

	[SerializeField]
	private CustomButton _backButton;

	[SerializeField]
	private ChallengePlayerDisplay _localPlayerDisplay;

	[SerializeField]
	private ChallengePlayerDisplay _enemyPlayerDisplay;

	[SerializeField]
	private Localize _challengeTitle;

	[SerializeField]
	private Transform _challengeInviteWindowPopupAnchor;

	private PVPChallengeController _challengeController;

	private ISocialManager _socialManager;

	private Guid _currentChallengeId;

	private ChallengeInviteWindowPopup _challengeInviteWindowPopup;

	private AssetLookupSystem _assetLookupSystem;

	private ISystemMessageManager _systemMessageManager;

	private void Awake()
	{
		_challengeController = Pantry.Get<PVPChallengeController>();
		_socialManager = Pantry.Get<ISocialManager>();
		_leaveButton.OnClick.AddListener(HandleLeaveButtonClicked);
		_leaveButton.SetText(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Challenges/LeaveButton/LeaveChallenge"), warnOnMissingTextComponent: false);
		_backButton.OnClick.AddListener(HandleLeaveButtonClicked);
		_assetLookupSystem = Pantry.Get<AssetLookupManager>().AssetLookupSystem;
		_systemMessageManager = Pantry.Get<ISystemMessageManager>();
		ChallengePlayerDisplay enemyPlayerDisplay = _enemyPlayerDisplay;
		enemyPlayerDisplay.KickButtonPressed = (Action<string>)Delegate.Combine(enemyPlayerDisplay.KickButtonPressed, new Action<string>(OnPlayerKickPressed));
		ChallengePlayerDisplay enemyPlayerDisplay2 = _enemyPlayerDisplay;
		enemyPlayerDisplay2.BlockButtonPressed = (Action<string>)Delegate.Combine(enemyPlayerDisplay2.BlockButtonPressed, new Action<string>(OnPlayerBlockPressed));
		ChallengePlayerDisplay enemyPlayerDisplay3 = _enemyPlayerDisplay;
		enemyPlayerDisplay3.AddFriendButtonPressed = (Action<string>)Delegate.Combine(enemyPlayerDisplay3.AddFriendButtonPressed, new Action<string>(OnAddFriendPressed));
		ChallengePlayerDisplay enemyPlayerDisplay4 = _enemyPlayerDisplay;
		enemyPlayerDisplay4.InviteButtonPressed = (Action)Delegate.Combine(enemyPlayerDisplay4.InviteButtonPressed, new Action(OnInvitePressed));
		_socialManager.FriendPresenceChanged += OnSocialFriendPresenceChanged;
		if (_challengeInviteWindowPopup == null)
		{
			DisplayChallengeInviteWindowPrefab();
		}
		_challengeInviteWindowPopup.Activate(activate: false);
	}

	public void OnEnable()
	{
		_challengeController.RegisterForChallengeChanges(HandleChallengeDataChange);
		ChallengeInviteWindowPopup challengeInviteWindowPopup = _challengeInviteWindowPopup;
		challengeInviteWindowPopup.InviteButtonPressed = (Action<ChallengeInviteWindowPlayerData>)Delegate.Combine(challengeInviteWindowPopup.InviteButtonPressed, new Action<ChallengeInviteWindowPlayerData>(OnChallengeInviteWindowInviteButtonPressed));
		if (_currentChallengeId != Guid.Empty)
		{
			PVPChallengeData challengeData = _challengeController.GetChallengeData(_currentChallengeId);
			if (challengeData != null)
			{
				UpdateView(challengeData);
			}
		}
	}

	private void OnDestroy()
	{
		_challengeController.UnRegisterForChallengeChanges(HandleChallengeDataChange);
		_leaveButton.OnClick.RemoveListener(HandleLeaveButtonClicked);
		_backButton.OnClick.RemoveListener(HandleLeaveButtonClicked);
		ChallengePlayerDisplay enemyPlayerDisplay = _enemyPlayerDisplay;
		enemyPlayerDisplay.KickButtonPressed = (Action<string>)Delegate.Remove(enemyPlayerDisplay.KickButtonPressed, new Action<string>(OnPlayerKickPressed));
		ChallengePlayerDisplay enemyPlayerDisplay2 = _enemyPlayerDisplay;
		enemyPlayerDisplay2.BlockButtonPressed = (Action<string>)Delegate.Remove(enemyPlayerDisplay2.BlockButtonPressed, new Action<string>(OnPlayerBlockPressed));
		ChallengePlayerDisplay enemyPlayerDisplay3 = _enemyPlayerDisplay;
		enemyPlayerDisplay3.AddFriendButtonPressed = (Action<string>)Delegate.Remove(enemyPlayerDisplay3.AddFriendButtonPressed, new Action<string>(OnAddFriendPressed));
		ChallengePlayerDisplay enemyPlayerDisplay4 = _enemyPlayerDisplay;
		enemyPlayerDisplay4.InviteButtonPressed = (Action)Delegate.Remove(enemyPlayerDisplay4.InviteButtonPressed, new Action(OnInvitePressed));
		_socialManager.FriendPresenceChanged -= OnSocialFriendPresenceChanged;
	}

	public void OnDisable()
	{
		_challengeController.UnRegisterForChallengeChanges(HandleChallengeDataChange);
		ChallengeInviteWindowPopup challengeInviteWindowPopup = _challengeInviteWindowPopup;
		challengeInviteWindowPopup.InviteButtonPressed = (Action<ChallengeInviteWindowPlayerData>)Delegate.Remove(challengeInviteWindowPopup.InviteButtonPressed, new Action<ChallengeInviteWindowPlayerData>(OnChallengeInviteWindowInviteButtonPressed));
		_challengeInviteWindowPopup.Activate(activate: false);
		_challengeInviteWindowPopup.SetDefaultUsername();
	}

	public void ViewChallenge(string playerId)
	{
		PVPChallengeData challengeData = _challengeController.GetChallengeData(playerId);
		if (challengeData != null)
		{
			ViewChallenge(challengeData.ChallengeId);
		}
	}

	public void ViewChallenge(Guid challengeId)
	{
		PVPChallengeData challengeData = _challengeController.GetChallengeData(challengeId);
		if (challengeData != null)
		{
			_currentChallengeId = challengeData.ChallengeId;
			UpdateView(challengeData);
		}
	}

	private void HandleChallengeDataChange(PVPChallengeData challengeData)
	{
		if (challengeData.ChallengeId == _currentChallengeId)
		{
			UpdateView(challengeData);
		}
	}

	private void OnPlayerKickPressed(string playerId)
	{
		_challengeController.KickPlayer(_currentChallengeId, playerId);
	}

	private void OnPlayerBlockPressed(string playerId)
	{
		_challengeController.BlockPlayer(_currentChallengeId, playerId);
	}

	private void OnAddFriendPressed(string playerId)
	{
		PVPChallengeData challengeData = _challengeController.GetChallengeData(_currentChallengeId);
		if (challengeData != null && challengeData.ChallengePlayers.TryGetValue(playerId, out var player))
		{
			_systemMessageManager.ShowOkCancel(Languages.ActiveLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/FriendTitle"), Languages.ActiveLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/FriendDescription", ("username", SharedUtilities.FormatDisplayName(player.FullDisplayName, Color.white, 0u))), delegate
			{
				_socialManager.SubmitFriendInviteOutgoing(player.FullDisplayName);
			}, delegate
			{
			});
		}
		else
		{
			SimpleLog.LogError("UnifiedChallengeDisplay: Adding as a friend failed, challenge or player not found.");
		}
	}

	private void OnInvitePressed()
	{
		_challengeInviteWindowPopup.Activate(activate: true);
		ChallengeInviteWindowPlayerData challengeInviteWindowPlayerData = CreateChallengeInviteWindowPlayerData(_challengeController.GetChallengeData(_currentChallengeId));
		_challengeInviteWindowPopup.RefreshView(challengeInviteWindowPlayerData);
		BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengeInviteWindowOpened.ToString()), ("ChallengeId", _currentChallengeId.ToString()));
	}

	private void DisplayChallengeInviteWindowPrefab()
	{
		ChallengeInviteWindowPlayerData challengeInviteWindowPlayerData = CreateChallengeInviteWindowPlayerData(_challengeController.GetChallengeData(_currentChallengeId));
		string prefabPath = _assetLookupSystem.GetPrefabPath<ChallengeInviteWindowPopupPrefab, ChallengeInviteWindowPopup>();
		_challengeInviteWindowPopup = AssetLoader.Instantiate<ChallengeInviteWindowPopup>(prefabPath, _challengeInviteWindowPopupAnchor);
		_challengeInviteWindowPopup.RefreshView(challengeInviteWindowPlayerData);
	}

	private void OnChallengeInviteWindowInviteButtonPressed(ChallengeInviteWindowPlayerData challengeInviteWindowPlayerData)
	{
		if (challengeInviteWindowPlayerData == null || challengeInviteWindowPlayerData.FriendPlayers == null || challengeInviteWindowPlayerData.FriendPlayers.Count <= 0)
		{
			return;
		}
		foreach (ChallengeInviteWindowPlayer friendPlayer in challengeInviteWindowPlayerData.FriendPlayers)
		{
			if (friendPlayer.Invited)
			{
				_challengeController.AddChallengeInvite(_currentChallengeId, friendPlayer.PlayerName, friendPlayer.PlayerId);
			}
		}
		_challengeController.SendChallengeInvites(_currentChallengeId);
	}

	public ChallengeInviteWindowPlayerData CreateChallengeInviteWindowPlayerData(PVPChallengeData challengeData)
	{
		ChallengeInviteWindowPlayerData challengeInviteWindowPlayerData = new ChallengeInviteWindowPlayerData();
		if (challengeData != null && challengeData.Invites != null)
		{
			List<ChallengeInvite> list = challengeData.Invites.Values.ToList();
			foreach (ChallengeInvite item in list.OrderByDescending((ChallengeInvite d) => d.InviteSentTime).ToList().GetRange(0, (list.Count < 10) ? list.Count : 10))
			{
				if (item.Status == InviteStatus.Sent)
				{
					challengeInviteWindowPlayerData.InvitedPlayers.Add(new ChallengeInviteWindowPlayer
					{
						Invited = true,
						PlayerId = item.Recipient.PlayerId,
						PlayerName = item.Recipient.FullDisplayName
					});
				}
			}
		}
		if (_socialManager != null && _socialManager.Friends != null)
		{
			foreach (SocialEntity friend in _socialManager.Friends)
			{
				if (friend.IsOnline && !friend.IsBlocked && !challengeInviteWindowPlayerData.InvitedPlayers.Where((ChallengeInviteWindowPlayer p) => p.PlayerId == friend.PlayerId).Any())
				{
					challengeInviteWindowPlayerData.FriendPlayers.Add(new ChallengeInviteWindowPlayer
					{
						Invited = false,
						PlayerId = friend.PlayerId,
						PlayerName = friend.FullName,
						Status = SocialEntity.PresenceLocalizedString(friend.Status)
					});
				}
			}
		}
		return challengeInviteWindowPlayerData;
	}

	private void OnSocialFriendPresenceChanged(SocialEntity socialEntity)
	{
		if (_challengeInviteWindowPopup.IsShowing)
		{
			ChallengeInviteWindowPlayerData challengeInviteWindowPlayerData = CreateChallengeInviteWindowPlayerData(_challengeController.GetChallengeData(_currentChallengeId));
			_challengeInviteWindowPopup.RefreshView(challengeInviteWindowPlayerData);
		}
	}

	private void UpdateView(PVPChallengeData challengeData)
	{
		string displayName = challengeData?.ChallengePlayers[challengeData.ChallengeOwnerId]?.FullDisplayName;
		MTGALocalizedString text = new MTGALocalizedString
		{
			Key = "MainNav/Challenges/Title",
			Parameters = new Dictionary<string, string> { 
			{
				"username",
				SharedUtilities.FormatDisplayName(displayName, 0u)
			} }
		};
		_challengeTitle.SetText(text);
		_localPlayerDisplay.PlayerId = challengeData.LocalPlayerId;
		_enemyPlayerDisplay.PlayerId = challengeData.OpponentPlayerId;
		_localPlayerDisplay.UpdateView(challengeData);
		_enemyPlayerDisplay.UpdateView(challengeData);
		if (_challengeInviteWindowPopup.IsShowing)
		{
			ChallengeInviteWindowPlayerData challengeInviteWindowPlayerData = CreateChallengeInviteWindowPlayerData(_challengeController.GetChallengeData(_currentChallengeId));
			_challengeInviteWindowPopup.RefreshView(challengeInviteWindowPlayerData);
		}
		UpdateLeaveButtons(challengeData);
	}

	private void UpdateLeaveButtons(PVPChallengeData challengeData)
	{
		if (challengeData.Status == ChallengeStatus.Starting)
		{
			_leaveButton.Interactable = false;
			_backButton.Interactable = false;
		}
		else
		{
			_leaveButton.Interactable = true;
			_backButton.Interactable = true;
		}
	}

	private void HandleLeaveButtonClicked()
	{
		if (_currentChallengeId != Guid.Empty)
		{
			_challengeController.LeaveChallenge(_currentChallengeId);
		}
	}
}
