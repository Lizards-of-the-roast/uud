using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.BI;
using Core.Code.ClientFeatureToggle;
using Core.Code.Promises;
using Core.Meta.MainNavigation.SystemMessage;
using Core.Shared.Code.PVPChallenge;
using MTGA.Social;
using Newtonsoft.Json.Linq;
using SharedClientCore.SharedClientCore.Code.PVPChallenge;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using UnityEngine;
using Wizards.Arena.Enums.System;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Models.ClientBusinessEvents;
using Wizards.Mtga;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Inventory;
using Wizards.Mtga.PrivateGame;
using Wizards.Mtga.PrivateGame.Challenges;
using Wizards.Unification.Models.DirectChallenge;
using Wotc.Mtga;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Meta.MainNavigation.Challenge;

public class PVPChallengeController : IDisposable
{
	public static int MATCH_START_COUNTDOWN_SECONDS = 10;

	public static int MATCH_START_COUNTDOWN_LOCK_SECONDS = 7;

	private ChallengeDataProvider _challengeDataProvider;

	private readonly DeckDataProvider _deckDataProvider;

	private readonly CosmeticsProvider _cosmeticsProvider;

	private ClientFeatureToggleDataProvider _clientFeatureToggleDataProvider;

	private readonly IChallengeDeckValidation _challengeDeckValidation;

	private readonly IAccountClient _accountClient;

	private readonly SceneLoader _sceneLoader;

	private readonly IChallengeCommunicationWrapper _challengeCommunicationWrapper;

	private readonly ConnectionManager _connectionManager;

	private readonly IBILogger _biLogger;

	private readonly ISystemMessageManager _systemMessageManager;

	private readonly IClientLocProvider _clientLocProvider;

	public ChallengeDataProvider.ChallengePermissionState ChallengePermissionState
	{
		get
		{
			return _challengeDataProvider.PermissionState;
		}
		set
		{
			_challengeDataProvider.PermissionState = value;
		}
	}

	public bool CanSetBlockNonFriendChallenges => _challengeDataProvider.PreferencesAvailable;

	public event Action<bool> ChallengeEnabledChanged;

	public PVPChallengeController(ChallengeDataProvider challengeDataProvider, IChallengeCommunicationWrapper challengeCommunicationWrapper, DeckDataProvider deckDataProvider, ISystemMessageManager systemMessageManager, IClientLocProvider clientLocProvider, CosmeticsProvider cosmeticsProvider, IChallengeDeckValidation challengeDeckValidation, ConnectionManager connectionManager, IAccountClient accountClient, IBILogger biLogger = null)
	{
		_challengeDataProvider = challengeDataProvider;
		_challengeCommunicationWrapper = challengeCommunicationWrapper;
		_deckDataProvider = deckDataProvider;
		_systemMessageManager = systemMessageManager;
		_clientLocProvider = clientLocProvider;
		_cosmeticsProvider = cosmeticsProvider;
		_challengeDeckValidation = challengeDeckValidation;
		_connectionManager = connectionManager;
		_accountClient = accountClient;
		_clientFeatureToggleDataProvider = Pantry.Get<ClientFeatureToggleDataProvider>();
		_biLogger = biLogger;
		IChallengeCommunicationWrapper challengeCommunicationWrapper2 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper2.OnIncomingChallenge = (Action<PVPChallengeData>)Delegate.Combine(challengeCommunicationWrapper2.OnIncomingChallenge, new Action<PVPChallengeData>(HandleChallengeInviteUpdate));
		IChallengeCommunicationWrapper challengeCommunicationWrapper3 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper3.OnChallengeResponse = (Action<Guid, string, InviteStatus>)Delegate.Combine(challengeCommunicationWrapper3.OnChallengeResponse, new Action<Guid, string, InviteStatus>(HandleChallengeInviteResponse));
		IChallengeCommunicationWrapper challengeCommunicationWrapper4 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper4.OnChallengeClosedMessage = (Action<Guid>)Delegate.Combine(challengeCommunicationWrapper4.OnChallengeClosedMessage, new Action<Guid>(HandleChallengeClosedMessage));
		IChallengeCommunicationWrapper challengeCommunicationWrapper5 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper5.OnChallengePlayerUpdate = (Action<Guid, ChallengePlayer>)Delegate.Combine(challengeCommunicationWrapper5.OnChallengePlayerUpdate, new Action<Guid, ChallengePlayer>(HandleChallengePlayerUpdate));
		IChallengeCommunicationWrapper challengeCommunicationWrapper6 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper6.OnChallengeGeneralUpdate = (Action<PVPChallengeData>)Delegate.Combine(challengeCommunicationWrapper6.OnChallengeGeneralUpdate, new Action<PVPChallengeData>(HandleChallengeGeneralUpdate));
		IChallengeCommunicationWrapper challengeCommunicationWrapper7 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper7.OnChallengePlayerKickMessage = (Action<Guid>)Delegate.Combine(challengeCommunicationWrapper7.OnChallengePlayerKickMessage, new Action<Guid>(HandleChallengePlayerKickMessage));
		IChallengeCommunicationWrapper challengeCommunicationWrapper8 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper8.OnChallengeCountdownStart = (Action<Guid, int>)Delegate.Combine(challengeCommunicationWrapper8.OnChallengeCountdownStart, new Action<Guid, int>(HandleMatchLaunch));
		IChallengeCommunicationWrapper challengeCommunicationWrapper9 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper9.OnPlayerBlocked = (Action<List<Block>>)Delegate.Combine(challengeCommunicationWrapper9.OnPlayerBlocked, new Action<List<Block>>(OnPlayerBlocked));
		OnChallengeEnabledChanged();
		_clientFeatureToggleDataProvider.RegisterForToggleUpdates(OnChallengeEnabledChanged);
		if (_connectionManager != null)
		{
			ConnectionManager connectionManager2 = _connectionManager;
			connectionManager2.OnFdReconnected = (Action)Delegate.Combine(connectionManager2.OnFdReconnected, new Action(HandleFrontdoorReconnect));
		}
	}

	private void HandleChallengePlayerKickMessage(Guid challengeId)
	{
		PVPChallengeData challengeData = GetChallengeData(challengeId);
		if (challengeData != null)
		{
			RemoveChallenge(challengeId);
			string displayName = challengeData?.ChallengePlayers[challengeData.ChallengeOwnerId]?.FullDisplayName;
			_systemMessageManager.ShowOk(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeClosedTitle"), _clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeClosedBody", ("username", SharedUtilities.FormatDisplayName(displayName, 0u))));
		}
	}

	private void HandleChallengeInviteUpdate(PVPChallengeData data)
	{
		_challengeDataProvider.GetBlockNonFriendChallenges().ThenOnMainThreadIfSuccess(delegate(bool blockNonFriendChallenges)
		{
			if (data.Invites.TryGetValue(data.LocalPlayerId, out var value))
			{
				if (!Pantry.Get<ISocialManager>().CheckIfAlreadyFriends(value.Sender.PlayerId) && blockNonFriendChallenges)
				{
					RejectAllNonFriendChallengeInvites();
				}
				else if (value.Status == InviteStatus.Sent)
				{
					SetChallengeData(data);
					BIMessage_FriendChallengeReceived(data);
				}
				else if (value.Status == InviteStatus.Cancelled)
				{
					RemoveChallenge(data.ChallengeId);
				}
			}
		}).ThenOnMainThreadIfError((Action<Error>)delegate
		{
			SimpleLog.LogError("PVPChallengeController: Promise Failure fetching BlockNonFriendChallenges");
		});
	}

	public Promise<bool> GetBlockNonFriendChallengesIncoming()
	{
		return _challengeDataProvider.GetBlockNonFriendChallenges();
	}

	public void ToggleBlockNonFriendChallengesIncoming(bool shouldBlock)
	{
		_challengeDataProvider.SetBlockNonFriendChallenges(shouldBlock).ThenOnMainThreadIfSuccess((Action<bool>)delegate
		{
			if (shouldBlock)
			{
				RejectAllNonFriendChallengeInvites();
			}
		});
	}

	private void HandleChallengeInviteResponse(Guid challengeId, string playerId, InviteStatus status)
	{
		PVPChallengeData challengeData = GetChallengeData(challengeId);
		if (challengeData == null)
		{
			return;
		}
		ChallengeInvite challengeInvite = challengeData.Invites[playerId];
		if (challengeInvite != null)
		{
			challengeInvite.Status = status;
			SetChallengeData(challengeData);
			if (challengeInvite.Status == InviteStatus.Rejected)
			{
				RemoveChallengeInvite(challengeId, playerId);
			}
		}
	}

	private void HandleChallengeClosedMessage(Guid challengeId)
	{
		PVPChallengeData challengeData = GetChallengeData(challengeId);
		if (challengeData != null)
		{
			challengeData.Status = ChallengeStatus.Removed;
			SetChallengeData(challengeData);
			RemoveChallenge(challengeId);
			RefreshNavBarButtons();
			BIMessage_FriendChallengeCanceled(challengeData);
			if (ChallengePermissionState != ChallengeDataProvider.ChallengePermissionState.Restricted_InMatch && challengeData.LocalPlayerId != challengeData.ChallengeOwnerId)
			{
				string displayName = challengeData?.ChallengePlayers[challengeData.ChallengeOwnerId]?.FullDisplayName;
				_systemMessageManager.ShowOk(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeClosedTitle"), _clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeClosedBody", ("username", SharedUtilities.FormatDisplayName(displayName, 0u))));
			}
		}
	}

	private void HandleChallengePlayerUpdate(Guid challengeId, ChallengePlayer player)
	{
		PVPChallengeData challengeData = GetChallengeData(challengeId);
		if (challengeData != null && challengeData.ChallengePlayers.ContainsKey(player.PlayerId))
		{
			challengeData.ChallengePlayers[player.PlayerId] = player;
			_challengeDataProvider.SetChallengeData(challengeData);
			CheckForSetup(challengeData);
		}
	}

	private bool CheckForSetup(PVPChallengeData challenge)
	{
		if (challenge.MatchLaunchCountdown == -1 || !AllPlayersReady(challenge))
		{
			challenge.Status = ChallengeStatus.Setup;
			challenge.MatchLaunchCountdown = -1;
			challenge.MatchLaunchDateTime = DateTime.MinValue;
			_challengeDataProvider.SetChallengeData(challenge);
			return true;
		}
		return false;
	}

	private void HandleChallengeGeneralUpdate(PVPChallengeData challenge)
	{
		if (!challenge.ChallengePlayers.ContainsKey(_accountClient.AccountInformation.PersonaID))
		{
			if (_challengeDataProvider.GetChallengeData(challenge.ChallengeId) != null)
			{
				RemoveChallenge(challenge.ChallengeId);
			}
			return;
		}
		ApplyLocalChallengeData(challenge);
		CheckForSetup(challenge);
		CheckForMatchLaunch(challenge);
		CleanupAcceptedInvites(challenge);
		SetChallengeData(challenge);
		if (!CheckLocalPlayerDeckValid(challenge.ChallengeId) && challenge.LocalPlayer.PlayerStatus == PlayerStatus.Ready)
		{
			SetLocalPlayerStatus(challenge.ChallengeId, PlayerStatus.NotReady);
		}
	}

	private void CleanupAcceptedInvites(PVPChallengeData challenge)
	{
		List<string> list = new List<string>();
		foreach (KeyValuePair<string, ChallengePlayer> challengePlayer in challenge.ChallengePlayers)
		{
			foreach (KeyValuePair<string, ChallengeInvite> invite in challenge.Invites)
			{
				if (challengePlayer.Key == invite.Key)
				{
					list.Add(invite.Key);
				}
			}
		}
		foreach (string item in list)
		{
			challenge.Invites.Remove(item);
		}
	}

	private void ApplyLocalChallengeData(PVPChallengeData challenge)
	{
		challenge.LocalPlayerId = _accountClient.AccountInformation.PersonaID;
		challenge.LocalPlayerDisplayName = _accountClient.AccountInformation.DisplayName;
		PVPChallengeData challengeData = GetChallengeData(challenge.ChallengeId);
		if (challengeData != null)
		{
			challenge.Invites = challengeData.Invites;
			if (challengeData.ChallengePlayers.TryGetValue(challenge.LocalPlayerId, out var value) && challenge.ChallengePlayers.TryGetValue(challenge.LocalPlayerId, out var value2))
			{
				value2.DeckId = value.DeckId;
				value2.DeckArtId = value.DeckArtId;
				value2.DeckTileId = value.DeckTileId;
				value2.Cosmetics = value.Cosmetics;
			}
		}
	}

	private async Task CheckForMatchLaunch(PVPChallengeData challenge)
	{
		if (challenge.Status == ChallengeStatus.Starting && challenge.MatchLaunchCountdown > 0 && challenge.MatchLaunchDateTime == DateTime.MinValue)
		{
			challenge.MatchLaunchDateTime = DateTime.Now.AddSeconds(challenge.MatchLaunchCountdown);
			await Task.Delay(TimeSpan.FromSeconds(MATCH_START_COUNTDOWN_LOCK_SECONDS));
			challenge = GetChallengeData(challenge.ChallengeId);
			if (challenge.Status == ChallengeStatus.Starting)
			{
				SetChallengeData(challenge);
			}
			await Task.Delay(TimeSpan.FromSeconds(challenge.MatchLaunchCountdown - MATCH_START_COUNTDOWN_LOCK_SECONDS));
			challenge = GetChallengeData(challenge.ChallengeId);
			if (challenge.Status == ChallengeStatus.Starting && challenge.MatchLaunchDateTime != DateTime.MinValue && DateTime.Now >= challenge.MatchLaunchDateTime)
			{
				challenge.Status = ChallengeStatus.WaitingForMatch;
				SetChallengeData(challenge);
				JoinChallengeMatch(challenge.ChallengeId);
			}
		}
		else
		{
			challenge.MatchLaunchDateTime = DateTime.MinValue;
		}
	}

	private void HandleFrontdoorReconnect()
	{
		ReconnectAndCleanupOldChallenges();
	}

	public Promise<bool> ReconnectAndCleanupOldChallenges()
	{
		SimplePromise<bool> reconnectPromise = new SimplePromise<bool>();
		_challengeCommunicationWrapper.ChallengeReconnectAll().ThenOnMainThread(delegate(Promise<List<PVPChallengeData>> promise)
		{
			if (promise.Successful)
			{
				foreach (PVPChallengeData item in promise.Result)
				{
					HandleChallengeGeneralUpdate(item);
				}
				Dictionary<Guid, PVPChallengeData> allChallenges = GetAllChallenges();
				List<Guid> list = new List<Guid>();
				if (allChallenges.Count > 0)
				{
					foreach (PVPChallengeData value in allChallenges.Values)
					{
						if (value.Invites.Count > 0)
						{
							foreach (ChallengeInvite value2 in value.Invites.Values)
							{
								if (value2.Recipient.PlayerId == value.LocalPlayerId && value2.Status == InviteStatus.Sent)
								{
									list.Add(value.ChallengeId);
								}
							}
						}
					}
				}
				foreach (Guid item2 in allChallenges.Keys.Except(promise.Result.Select((PVPChallengeData x) => x.ChallengeId)).Except(list).ToList())
				{
					RemoveChallenge(item2);
				}
				reconnectPromise.SetResult(result: true);
			}
			else
			{
				if (promise.Error.Code == 9000)
				{
					foreach (Guid item3 in GetAllChallenges().Keys.ToList())
					{
						RemoveChallenge(item3);
					}
					RefreshNavBarButtons();
				}
				else
				{
					SimpleLog.LogError("PVPChallengeController: Failed to reconnect to all challenges");
				}
				reconnectPromise.SetResult(result: false);
			}
		});
		return reconnectPromise;
	}

	public bool IsChallengeEnabled()
	{
		return _clientFeatureToggleDataProvider.GetToggleValueById("Challenges");
	}

	public void OnChallengeEnabledChanged()
	{
		if (!IsChallengeEnabled())
		{
			ChallengePermissionState = ChallengeDataProvider.ChallengePermissionState.Restricted_ChallengesKillswitched;
			foreach (Guid item in GetAllChallenges().Keys.ToList())
			{
				RemoveChallenge(item);
			}
		}
		else if (ChallengePermissionState == ChallengeDataProvider.ChallengePermissionState.Restricted_ChallengesKillswitched)
		{
			ChallengePermissionState = ChallengeDataProvider.ChallengePermissionState.Normal;
		}
		this.ChallengeEnabledChanged?.Invoke(IsChallengeEnabled());
	}

	public PVPChallengeData GetChallengeData(string playerId)
	{
		return _challengeDataProvider.GetChallengeData(playerId);
	}

	public PVPChallengeData GetChallengeData(Guid challengeId)
	{
		return _challengeDataProvider.GetChallengeData(challengeId);
	}

	public void SetChallengeData(PVPChallengeData data)
	{
		_challengeDataProvider.SetChallengeData(data);
	}

	public Dictionary<Guid, PVPChallengeData> GetAllChallenges()
	{
		return _challengeDataProvider.GetAllChallenges();
	}

	public PVPChallengeData GetActiveCurrentChallengeData()
	{
		foreach (var (_, pVPChallengeData2) in GetAllChallenges())
		{
			if (pVPChallengeData2.ChallengePlayers.ContainsKey(pVPChallengeData2.LocalPlayerId))
			{
				return pVPChallengeData2;
			}
		}
		return null;
	}

	public List<PVPChallengeData> GetIncomingChallengeRequests()
	{
		List<PVPChallengeData> list = new List<PVPChallengeData>();
		foreach (KeyValuePair<Guid, PVPChallengeData> allChallenge in GetAllChallenges())
		{
			allChallenge.Deconstruct(out var _, out var value);
			PVPChallengeData pVPChallengeData = value;
			string key2 = _accountClient?.AccountInformation?.PersonaID ?? pVPChallengeData.LocalPlayerId;
			if (pVPChallengeData.Invites.ContainsKey(key2))
			{
				list.Add(pVPChallengeData);
			}
		}
		list.Sort((PVPChallengeData x, PVPChallengeData y) => x.Invites[x.LocalPlayerId].InviteSentTime.CompareTo(y.Invites[y.LocalPlayerId].InviteSentTime));
		return list;
	}

	public void RegisterForChallengeChanges(Action<PVPChallengeData> handler, bool forceOnMainThread = true)
	{
		_challengeDataProvider.RegisterForChallengeChanges(handler, forceOnMainThread);
	}

	public void UnRegisterForChallengeChanges(Action<PVPChallengeData> handler)
	{
		_challengeDataProvider.UnRegisterForChallengeChanges(handler);
	}

	public bool IsChallengeLocked(Guid challengeId)
	{
		PVPChallengeData challengeData = GetChallengeData(challengeId);
		if (challengeData != null && challengeData.Status == ChallengeStatus.Starting && challengeData.MatchLaunchDateTime != DateTime.MinValue && (challengeData.MatchLaunchDateTime - DateTime.Now).TotalSeconds <= (double)(MATCH_START_COUNTDOWN_SECONDS - MATCH_START_COUNTDOWN_LOCK_SECONDS))
		{
			return true;
		}
		return false;
	}

	public void LaunchChallenge(Guid challengeId)
	{
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(challengeId);
		if (challengeData == null)
		{
			Guid guid = challengeId;
			SimpleLog.LogError("PVPChallengeController: LaunchChallenge failed, challenge does not exist. ChallengeId: " + guid.ToString());
		}
		else if (challengeData.Status == ChallengeStatus.Starting)
		{
			Guid guid = challengeId;
			SimpleLog.LogError("PVPChallengeController: LaunchChallenge failed, challenge is already starting. ChallengeId: " + guid.ToString());
		}
		else if (challengeData.LocalPlayerId != challengeData.ChallengeOwnerId)
		{
			Guid guid = challengeId;
			SimpleLog.LogError("PVPChallengeController: LaunchChallenge failed, you are not the owner. ChallengeId: " + guid.ToString());
		}
		else if (!CheckLocalPlayerDeckValid(challengeId))
		{
			Guid guid = challengeId;
			SimpleLog.LogError("PVPChallengeController: LaunchChallenge failed, local player deck is not valid. ChallengeId: " + guid.ToString());
		}
		else
		{
			_challengeCommunicationWrapper.LaunchChallenge(challengeData);
		}
	}

	private void HandleMatchLaunch(Guid challengeId, int countdownSeconds)
	{
		PVPChallengeData challengeData = GetChallengeData(challengeId);
		if (challengeData == null)
		{
			SimpleLog.LogError("PVPChallengeController: HandleMatchLaunch failed to find the challenge!");
			return;
		}
		challengeData.MatchLaunchCountdown = countdownSeconds;
		challengeData.Status = ChallengeStatus.Starting;
		CheckForMatchLaunch(challengeData);
		SetChallengeData(challengeData);
	}

	public void JoinChallengeMatch(Guid challengeId)
	{
		PVPChallengeData challenge = _challengeDataProvider.GetChallengeData(challengeId);
		if (challenge == null || challenge.Status != ChallengeStatus.WaitingForMatch)
		{
			return;
		}
		Client_Deck deck = _deckDataProvider.GetDeckForId(challenge.LocalPlayer.DeckId);
		if (deck != null)
		{
			WrapperDeckUtilities.setLastPlayed(deck);
		}
		WrapperController.EnableLoadingIndicator(enabled: true);
		_challengeCommunicationWrapper.IssueChallenge(_challengeDataProvider.GetChallengeData(challengeId)).ThenOnMainThread(delegate(Promise<PVPChallengeData> result)
		{
			if (result.Error.IsError)
			{
				HandleIssueChallengeError(result.Error, challenge);
			}
			else
			{
				EventContext privateGameEventContext = WrapperController.Instance.EventManager.PrivateGameEventContext;
				_challengeCommunicationWrapper.JoinChallengeMatch(privateGameEventContext, challenge);
				AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_play_match_start.EventName, AudioManager.Default);
				privateGameEventContext.PlayerEvent.CourseData.CourseDeck = deck;
				RejectAllChallengeInvites(challengeId);
				CancelAllChallenges(challengeId);
			}
			WrapperController.EnableLoadingIndicator(enabled: false);
		});
	}

	private void HandleIssueChallengeError(Error error, PVPChallengeData challenge)
	{
		Dictionary<string, object> data = error.Data;
		EMismatchReason[] source;
		if (data != null && data.TryGetValue("reasons", out var value) && value is JArray jArray)
		{
			EMismatchReason[] array = jArray.ToObject<EMismatchReason[]>();
			if (array != null)
			{
				source = array;
				goto IL_0035;
			}
		}
		source = Array.Empty<EMismatchReason>();
		goto IL_0035;
		IL_0035:
		EDirectChallengeMismatch[] reasons = source.Select(MismatchToDirectChallengeMismatch).ToArray();
		SimpleLog.LogError("PVPChallengeController: Failed to join challenge " + error.ToString() + "\n" + challenge);
		ServerErrors code = (ServerErrors)error.Code;
		string errTitle;
		string errText;
		if (code == ServerErrors.InvalidParams)
		{
			(errTitle, errText) = Utils.GetChallengeErrorMessages(reasons);
		}
		else
		{
			Utils.GetDeckSubmissionErrorMessages(code, out errTitle, out errText);
		}
		_systemMessageManager.ShowOk(errTitle, errText);
		if (_deckDataProvider != null && error.Code == 7000)
		{
			_deckDataProvider.MarkDirty();
			_deckDataProvider.GetAllDecks();
		}
		LeaveChallenge(challenge.ChallengeId, confirm: false);
		static EDirectChallengeMismatch MismatchToDirectChallengeMismatch(EMismatchReason reason)
		{
			return reason switch
			{
				EMismatchReason.VariantMismatch => EDirectChallengeMismatch.VariantMismatch, 
				EMismatchReason.Bo3Mismatch => EDirectChallengeMismatch.BestOf3Mismatch, 
				EMismatchReason.GoFirstMismatch => EDirectChallengeMismatch.PlayFirstMismatch, 
				_ => throw new ArgumentOutOfRangeException("reason", reason, null), 
			};
		}
	}

	public void CloseChallenge(Guid challengeId)
	{
		PVPChallengeData challengeData = GetChallengeData(challengeId);
		if (challengeData == null)
		{
			return;
		}
		foreach (KeyValuePair<string, ChallengeInvite> invite in challengeData.Invites)
		{
			if (invite.Value.Status == InviteStatus.Sent)
			{
				CancelChallengeInvite(challengeId, invite.Key);
			}
		}
		_challengeCommunicationWrapper.CloseChallenge(challengeData);
		if (_biLogger != null)
		{
			BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengeClosed.ToString()), ("ChallengeId", challengeData.ChallengeId.ToString()));
		}
	}

	public void SetDeckForChallengeFromPrefs(Guid challengeId)
	{
		string selectedDeckId = MDNPlayerPrefs.GetSelectedDeckId(_accountClient.AccountInformation?.PersonaID, "DirectGame");
		if (!string.IsNullOrEmpty(selectedDeckId))
		{
			SetDeckForChallenge(challengeId, Guid.Parse(selectedDeckId));
		}
	}

	public void SetDeckForChallenge(Guid challengeId, Guid deckId)
	{
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(challengeId);
		if (challengeData != null && challengeData.LocalPlayer != null && challengeData.LocalPlayer.DeckId != deckId && challengeData.Status == ChallengeStatus.Setup)
		{
			challengeData.LocalPlayer.DeckId = deckId;
			challengeData.LocalPlayer.DeckArtId = GetDeckArtId(deckId);
			challengeData.LocalPlayer.DeckTileId = GetDeckTileId(deckId);
			challengeData.LocalPlayer.Cosmetics = GetLocalPlayerCosmetics(challengeData);
			_challengeDataProvider.SetChallengeData(challengeData);
			if (challengeData.LocalPlayer.PlayerStatus == PlayerStatus.Ready)
			{
				SetLocalPlayerStatus(challengeId, PlayerStatus.NotReady);
			}
		}
	}

	private ClientVanitySelectionsV3 GetLocalPlayerCosmetics(PVPChallengeData challenge)
	{
		if (_cosmeticsProvider == null)
		{
			return new ClientVanitySelectionsV3();
		}
		ClientVanitySelectionsV3 clientVanitySelectionsV = new ClientVanitySelectionsV3();
		clientVanitySelectionsV.titleSelection = ChallengeDataUtils.GetTitleLocKey(_cosmeticsProvider.PlayerTitleSelection, _cosmeticsProvider);
		clientVanitySelectionsV.avatarSelection = _cosmeticsProvider.PlayerAvatarSelection;
		clientVanitySelectionsV.cardBackSelection = _cosmeticsProvider.PlayerCardbackSelection;
		clientVanitySelectionsV.petSelection = _cosmeticsProvider.PlayerPetSelection;
		if (challenge != null && challenge.LocalPlayer != null && challenge.LocalPlayer.DeckId != Guid.Empty)
		{
			Client_Deck deckForId = _deckDataProvider.GetDeckForId(challenge.LocalPlayer.DeckId);
			if (deckForId != null)
			{
				if (!string.IsNullOrEmpty(deckForId.Summary.Avatar))
				{
					clientVanitySelectionsV.avatarSelection = deckForId.Summary.Avatar;
				}
				if (!string.IsNullOrEmpty(deckForId.Summary.CardBack))
				{
					clientVanitySelectionsV.cardBackSelection = deckForId.Summary.CardBack;
				}
				if (!string.IsNullOrEmpty(deckForId.Summary.Pet))
				{
					ClientPetSelection obj = new ClientPetSelection
					{
						name = deckForId.Summary.Pet.Split(".")[0]
					};
					string[] array = deckForId.Summary.Pet.Split(".");
					obj.variant = ((array != null) ? array[1] : null);
					clientVanitySelectionsV.petSelection = obj;
				}
			}
		}
		return clientVanitySelectionsV;
	}

	public void SetLocalPlayerStatus(Guid challengeId, PlayerStatus playerStatus)
	{
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(challengeId);
		if (challengeData == null || challengeData.LocalPlayer == null || playerStatus == challengeData.LocalPlayer.PlayerStatus || (playerStatus == PlayerStatus.Ready && !CheckLocalPlayerDeckValid(challengeId)) || IsChallengeLocked(challengeId))
		{
			return;
		}
		challengeData.LocalPlayer.Cosmetics = GetLocalPlayerCosmetics(challengeData);
		if (challengeData.LocalPlayer.DeckId != Guid.Empty)
		{
			challengeData.LocalPlayer.DeckArtId = GetDeckArtId(challengeData.LocalPlayer.DeckId);
			challengeData.LocalPlayer.DeckTileId = GetDeckTileId(challengeData.LocalPlayer.DeckId);
		}
		_challengeCommunicationWrapper.SendPlayerUpdate(challengeData, challengeData.LocalPlayerId, playerStatus).ThenOnMainThread(delegate(Promise<PVPChallengeData> promise)
		{
			if (promise.Successful)
			{
				HandleChallengeGeneralUpdate(promise.Result);
			}
			else
			{
				SimpleLog.LogError($"PVPChallengeController: SetLocalPlayerStatus failed. Error: {promise.Error}");
			}
		});
		if (_biLogger != null)
		{
			BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengePlayerStatusChanged.ToString()), ("ChallengeId", challengeData.ChallengeId.ToString()), ("ChallengePlayerId", challengeData.LocalPlayerId), ("ChallengePlayerDisplayName", challengeData.LocalPlayer.FullDisplayName), ("ChallengePlayerStatus", playerStatus.ToString()));
		}
	}

	public bool CheckLocalPlayerDeckValid(Guid challengeId)
	{
		PVPChallengeData challengeData = GetChallengeData(challengeId);
		if (challengeData == null || challengeData.LocalPlayer == null || challengeData.LocalPlayer.DeckId == Guid.Empty || _deckDataProvider?.GetDeckForId(challengeData.LocalPlayer.DeckId) == null)
		{
			return false;
		}
		if (!ChallengeUtils.MatchTypeToInfo.TryGetValue(challengeData.MatchType, out var value))
		{
			SimpleLog.LogError($"PVPChallengeController: CheckLocalPlayerDeckValid failed to find format for challenge match type {challengeData.MatchType}");
			return false;
		}
		return _challengeDeckValidation.ValidateDeck(challengeData.LocalPlayer.DeckId, value.DeckFormatName).IsValid;
	}

	public void SetGameSettings(Guid challengeId, ChallengeMatchTypes matchType, WhoPlaysFirst whoPlaysFirst, bool isBestOf3)
	{
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(challengeId);
		if (challengeData == null)
		{
			Guid challengeId2 = challengeData.ChallengeId;
			SimpleLog.LogError("PVPChallengeController: SetGameSettings failed, challenge does not exist. ChallengeId: " + challengeId2.ToString());
			return;
		}
		if (challengeData.LocalPlayerId != challengeData.ChallengeOwnerId)
		{
			Guid challengeId2 = challengeData.ChallengeId;
			SimpleLog.LogError("PVPChallengeController: SetGameSettings failed, you are not the owner. ChallengeId: " + challengeId2.ToString());
			return;
		}
		if (challengeData.Status != ChallengeStatus.Setup)
		{
			Guid challengeId2 = challengeId;
			SimpleLog.LogError("PVPChallengeController: SetGameSettings failed, challenge is not in setup mode. ChallengeId: " + challengeId2.ToString());
			return;
		}
		if (IsChallengeLocked(challengeId))
		{
			Guid challengeId2 = challengeId;
			SimpleLog.LogError("PVPChallengeController: SetGameSettings failed, challenge is locked. ChallengeId: " + challengeId2.ToString());
			return;
		}
		SetLocalPlayerStatus(challengeId, PlayerStatus.NotReady);
		_challengeCommunicationWrapper.SetGameSettings(challengeData.ChallengeId.ToString(), matchType, whoPlaysFirst, isBestOf3).ThenOnMainThread(delegate(Promise<PVPChallengeData> promise)
		{
			if (promise.Successful)
			{
				HandleChallengeGeneralUpdate(promise.Result);
			}
			else
			{
				SimpleLog.LogError("PVPChallengeController: SetGameSettings failed");
			}
		});
	}

	public bool AllPlayersReady(PVPChallengeData challenge)
	{
		if (challenge != null && challenge.ChallengePlayers.Count >= 2)
		{
			foreach (KeyValuePair<string, ChallengePlayer> challengePlayer in challenge.ChallengePlayers)
			{
				if (challengePlayer.Value.PlayerStatus != PlayerStatus.Ready)
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	public void RemoveChallenge(Guid challengeId)
	{
		_challengeDataProvider.RemoveChallengeData(challengeId);
		RefreshNavBarButtons();
	}

	public void BlockPlayer(Guid challengeId, string playerId, bool confirm = true)
	{
		PVPChallengeData challenge = _challengeDataProvider.GetChallengeData(challengeId);
		string displayName = challenge.ChallengePlayers[playerId].FullDisplayName;
		string localizedText = _clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/BlockDescriptionHost", ("username", SharedUtilities.FormatDisplayName(displayName, Color.white, 0u)));
		if (challenge.ChallengeOwnerId != _accountClient.AccountInformation.PersonaID)
		{
			localizedText = _clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/BlockDescriptionInvitee", ("username", SharedUtilities.FormatDisplayName(displayName, Color.white, 0u)));
		}
		if (confirm)
		{
			_systemMessageManager.ShowOkCancel(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/BlockTitle"), localizedText, delegate
			{
				_challengeCommunicationWrapper.BlockPlayer(challenge, displayName);
			}, delegate
			{
			});
		}
		else
		{
			_challengeCommunicationWrapper.BlockPlayer(challenge, displayName);
		}
	}

	public void OnPlayerBlocked(List<Block> allBlocks)
	{
		foreach (Block allBlock in allBlocks)
		{
			string blockedPlayerId = allBlock.BlockedPlayer.PlayerId;
			if (blockedPlayerId == null)
			{
				continue;
			}
			Dictionary<Guid, PVPChallengeData> allChallenges = GetAllChallenges();
			foreach (Guid item in new List<Guid>(allChallenges.Keys))
			{
				if (!allChallenges.TryGetValue(item, out var challenge))
				{
					continue;
				}
				if (challenge.ChallengePlayers.ContainsKey(challenge.LocalPlayerId))
				{
					if (challenge.ChallengeOwnerId == blockedPlayerId)
					{
						LeaveChallenge(item, confirm: false);
					}
					else
					{
						KickPlayer(item, blockedPlayerId, confirm: false);
					}
				}
				else if (challenge.Invites.Exists((KeyValuePair<string, ChallengeInvite> invitePair) => invitePair.Value.Recipient.PlayerId == challenge.LocalPlayerId && invitePair.Value.Sender.PlayerId == blockedPlayerId))
				{
					RejectChallengeInvite(item);
				}
			}
		}
	}

	public void KickPlayer(Guid challengeId, string playerId, bool confirm = true)
	{
		PVPChallengeData challenge = _challengeDataProvider.GetChallengeData(challengeId);
		if (challenge == null || challenge.LocalPlayer.PlayerId == playerId || !challenge.ChallengePlayers.TryGetValue(playerId, out var kickPlayer))
		{
			return;
		}
		if (confirm)
		{
			_systemMessageManager.ShowOkCancel(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/KickTitle"), _clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/KickDescription", ("username", SharedUtilities.FormatDisplayName(kickPlayer.FullDisplayName, Color.white, 0u))), delegate
			{
				KickPlayerInternal(challenge, kickPlayer);
			}, delegate
			{
			});
		}
		else
		{
			KickPlayerInternal(challenge, kickPlayer);
		}
	}

	private void KickPlayerInternal(PVPChallengeData challenge, ChallengePlayer player)
	{
		_challengeCommunicationWrapper.KickPlayer(challenge, player).ThenOnMainThread(delegate(Promise<PVPChallengeData> promise)
		{
			if (promise.Successful)
			{
				HandleChallengeGeneralUpdate(promise.Result);
				if (_biLogger != null)
				{
					BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengePlayerKicked.ToString()), ("ChallengeId", challenge.ChallengeId.ToString()), ("PlayerKickedId", player.PlayerId), ("PlayerKickingId", challenge.LocalPlayerId), ("ChallengeOwnerId", challenge.ChallengeOwnerId));
				}
			}
			else
			{
				SimpleLog.LogError("PVPChallengeController: KickPlayerInternal: Failed to kick player");
			}
		});
	}

	public bool LeaveChallenge(Guid challengeId, bool confirm = true, Action screenChangeAction = null)
	{
		PVPChallengeData challenge = _challengeDataProvider.GetChallengeData(challengeId);
		bool successfullyLeft = false;
		if (challenge != null)
		{
			if (challenge.LocalPlayerId == challenge.ChallengeOwnerId)
			{
				if (confirm)
				{
					_systemMessageManager.ShowOkCancel(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/LeaveChallengeTitle"), _clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/LeaveDescriptionHost"), delegate
					{
						CloseChallenge(challengeId);
						screenChangeAction?.Invoke();
						if (_biLogger != null)
						{
							BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengePlayerLeave.ToString()), ("ChallengeId", challenge.ChallengeId.ToString()));
						}
						successfullyLeft = true;
					}, delegate
					{
						successfullyLeft = false;
					});
				}
				else
				{
					CloseChallenge(challengeId);
					screenChangeAction?.Invoke();
					if (_biLogger != null)
					{
						BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengePlayerLeave.ToString()), ("ChallengeId", challenge.ChallengeId.ToString()));
					}
					successfullyLeft = true;
				}
			}
			else if (confirm)
			{
				_systemMessageManager.ShowOkCancel(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/LeaveChallengeTitle"), _clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/LeaveDescriptionInvitee"), delegate
				{
					NonOwnerLeave(challenge);
					screenChangeAction?.Invoke();
					if (_biLogger != null)
					{
						BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengePlayerLeave.ToString()), ("ChallengeId", challenge.ChallengeId.ToString()));
					}
					successfullyLeft = true;
				}, delegate
				{
					successfullyLeft = false;
				});
			}
			else
			{
				NonOwnerLeave(challenge);
				screenChangeAction?.Invoke();
				if (_biLogger != null)
				{
					BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengePlayerLeave.ToString()), ("ChallengeId", challenge.ChallengeId.ToString()));
				}
				successfullyLeft = true;
			}
		}
		return successfullyLeft;
	}

	private void NonOwnerLeave(PVPChallengeData challenge)
	{
		_challengeCommunicationWrapper.LeaveChallenge(challenge).ThenOnMainThread(delegate(Promise<PVPChallengeData> promise)
		{
			if (promise.Successful || promise.Error.Code == 103)
			{
				RemoveChallenge(challenge.ChallengeId);
				RefreshNavBarButtons();
			}
			else
			{
				SimpleLog.LogError("PVPChallengeController: Failed to leave challenge");
			}
		});
	}

	public void AddChallengeInvite(Guid challengeId, string fullDisplayName, string playerId)
	{
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(challengeId);
		if (challengeData != null && challengeData.Status == ChallengeStatus.Setup)
		{
			if (!challengeData.Invites.ContainsKey(playerId))
			{
				if (challengeData.IsChallengeFull)
				{
					SimpleLog.LogError("PVPChallengeController: Failed to add challenge invite, challenge is full");
					return;
				}
				challengeData.Invites.Add(playerId, new ChallengeInvite
				{
					Sender = challengeData.LocalPlayer,
					Recipient = new ChallengePlayer
					{
						PlayerId = playerId,
						FullDisplayName = fullDisplayName
					},
					Status = InviteStatus.None,
					InviteSentTime = DateTime.Now
				});
			}
			else
			{
				challengeData.Invites[playerId].Status = InviteStatus.None;
			}
			_challengeDataProvider.SetChallengeData(challengeData);
		}
		else
		{
			SimpleLog.LogError("PVPChallengeController: Failed to add challenge invite");
		}
	}

	public void RemoveChallengeInvite(Guid challengeId, string playerId)
	{
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(challengeId);
		if (challengeData != null && challengeData.Status == ChallengeStatus.Setup)
		{
			challengeData.Invites.Remove(playerId);
			SetChallengeData(challengeData);
		}
	}

	public Promise<bool> SendChallengeInvites(Guid challengeId)
	{
		PVPChallengeData challenge = _challengeDataProvider.GetChallengeData(challengeId);
		if (challenge != null && challenge.Status == ChallengeStatus.Setup)
		{
			foreach (KeyValuePair<string, ChallengeInvite> invite in challenge.Invites)
			{
				if (invite.Value.Status == InviteStatus.None)
				{
					invite.Value.Status = InviteStatus.Sent;
					_challengeCommunicationWrapper.UpdateChallengeInvite(challenge, invite.Value).ThenOnMainThreadIfError((Action<Error>)delegate
					{
						_systemMessageManager.ShowOk(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeErrorTitle"), _clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeErrorBody"));
						invite.Value.Status = InviteStatus.None;
						SetChallengeData(challenge);
					});
					if (_biLogger != null)
					{
						BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengeInviteSent.ToString()), ("ChallengeId", challengeId.ToString()), ("ChallengePlayerSenderId", invite.Value.Sender.PlayerId), ("ChallengePlayerRecipientId", invite.Value.Recipient.PlayerId), ("InviteStatus", invite.Value.Status.ToString()), ("InviteSentTime", invite.Value.InviteSentTime.ToString()));
					}
				}
			}
			SetChallengeData(challenge);
		}
		return new SimplePromise<bool>(result: true);
	}

	public void CancelChallengeInvite(string playerId)
	{
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(playerId);
		if (challengeData != null)
		{
			CancelChallengeInvite(challengeData.ChallengeId, playerId);
		}
	}

	public void CancelChallengeInvite(Guid challengeId, string playerId)
	{
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(challengeId);
		if (challengeData != null && challengeData.Invites.TryGetValue(playerId, out var value))
		{
			value.Status = InviteStatus.Cancelled;
			SetChallengeData(challengeData);
			_challengeCommunicationWrapper.UpdateChallengeInvite(challengeData, value);
			if (_biLogger != null)
			{
				BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengeInviteCancelled.ToString()), ("ChallengeId", challengeData.ChallengeId.ToString()), ("ChallengePlayerSenderId", value.Sender.PlayerId), ("ChallengePlayerRecipientId", value.Recipient.PlayerId), ("InviteStatus", value.Status.ToString()), ("InviteSentTime", value.InviteSentTime.ToString()));
			}
		}
	}

	public void RejectAllNonFriendChallengeInvites()
	{
		if (_challengeCommunicationWrapper == null)
		{
			return;
		}
		Dictionary<Guid, PVPChallengeData>.ValueCollection values = _challengeDataProvider.GetAllChallenges().Values;
		List<SocialEntity> friends = _challengeCommunicationWrapper.GetFriends();
		List<Guid> list = new List<Guid>();
		foreach (PVPChallengeData item in values)
		{
			if (item.Invites.TryGetValue(item.LocalPlayerId, out var invite) && !friends.Exists((SocialEntity f) => f.PlayerId == invite.Sender.PlayerId))
			{
				list.Add(item.ChallengeId);
			}
		}
		foreach (Guid item2 in list)
		{
			RejectChallengeInvite(item2);
		}
	}

	public void RejectAllChallengeInvites(Guid challengeId = default(Guid))
	{
		if (_challengeDataProvider == null)
		{
			return;
		}
		PVPChallengeData[] array = _challengeDataProvider.GetAllChallenges().Values.Where((PVPChallengeData pVPChallengeData2) => pVPChallengeData2.ChallengeId != challengeId).ToArray();
		foreach (PVPChallengeData pVPChallengeData in array)
		{
			if (pVPChallengeData.Invites.TryGetValue(pVPChallengeData.LocalPlayerId, out var value) && value.Status == InviteStatus.Sent)
			{
				RejectChallengeInvite(pVPChallengeData.ChallengeId);
			}
		}
	}

	public void CancelAllChallenges(Guid challengeId = default(Guid))
	{
		if (_challengeDataProvider != null)
		{
			PVPChallengeData[] array = _challengeDataProvider.GetAllChallenges().Values.Where((PVPChallengeData value) => value.ChallengeId != challengeId).ToArray();
			foreach (PVPChallengeData pVPChallengeData in array)
			{
				CloseChallenge(pVPChallengeData.ChallengeId);
			}
		}
	}

	public Promise<PVPChallengeData> AcceptChallengeInvite(Guid challengeId)
	{
		if (GetActiveCurrentChallengeData() != null)
		{
			SystemMessageManager.Instance.ShowOk(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/ActiveChallengeTitle"), _clientLocProvider.GetLocalizedText("MainNav/Challenges/ConfirmationPanel/ActiveChallengeDescription"));
			SimplePromise<PVPChallengeData> simplePromise = new SimplePromise<PVPChallengeData>();
			simplePromise.SetError(new Error(0, "PVPChallengeController: Failed to accept challenge, player is already in another challenge"));
			return simplePromise;
		}
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(challengeId);
		if (challengeData != null && CanOpenChallenge(challengeId) && challengeData.Invites.TryGetValue(challengeData.LocalPlayerId, out var value))
		{
			if (_biLogger != null)
			{
				BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengeInviteAccepted.ToString()), ("ChallengeId", challengeData.ChallengeId.ToString()), ("ChallengePlayerSenderId", value.Sender.PlayerId), ("ChallengePlayerRecipientId", value.Recipient.PlayerId), ("InviteStatus", value.Status.ToString()), ("InviteSentTime", value.InviteSentTime.ToString()));
			}
			value.Status = InviteStatus.Accepted;
			SetChallengeData(challengeData);
			return _challengeCommunicationWrapper.RespondToChallengeInvite(challengeData.ChallengeId, value).ThenOnMainThread(delegate(Promise<PVPChallengeData> result)
			{
				if (result.Successful && result.Result != null)
				{
					HandleChallengeGeneralUpdate(result.Result);
					SetDeckForChallengeFromPrefs(challengeId);
				}
				else if (!result.Successful)
				{
					SystemMessageManager.Instance.ShowOk(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeErrorTitle"), _clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeErrorBody"));
				}
			});
		}
		SimplePromise<PVPChallengeData> simplePromise2 = new SimplePromise<PVPChallengeData>();
		simplePromise2.SetError(new Error(0, "There was a client error accepting the challenge invite"));
		return simplePromise2;
	}

	public void RejectChallengeInvite(Guid challengeId)
	{
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(challengeId);
		if (challengeData != null && challengeData.Invites.TryGetValue(challengeData.LocalPlayerId, out var value))
		{
			if (_biLogger != null)
			{
				BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengeInviteRejected.ToString()), ("ChallengeId", challengeData.ChallengeId.ToString()), ("ChallengePlayerSenderId", value.Sender.PlayerId), ("ChallengePlayerRecipientId", value.Recipient.PlayerId), ("InviteStatus", value.Status.ToString()), ("InviteSentTime", value.InviteSentTime.ToString()));
			}
			value.Status = InviteStatus.Rejected;
			RemoveChallenge(challengeId);
			_challengeCommunicationWrapper.RespondToChallengeInvite(challengeData.ChallengeId, value).ThenOnMainThreadIfError((Action<Error>)delegate
			{
				SystemMessageManager.Instance.ShowOk(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeErrorTitle"), _clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeErrorBody"));
			});
		}
	}

	public bool CanOpenChallenge(string playerId)
	{
		PVPChallengeData challengeData = _challengeDataProvider.GetChallengeData(playerId);
		if (challengeData != null)
		{
			return CanOpenChallenge(challengeData.ChallengeId);
		}
		return false;
	}

	public bool CanOpenChallenge(Guid challengeId)
	{
		if (_challengeDataProvider.GetChallengeData(challengeId) != null)
		{
			if (ChallengePermissionState == ChallengeDataProvider.ChallengePermissionState.Restricted_InMatch || ChallengePermissionState == ChallengeDataProvider.ChallengePermissionState.Restricted_ChallengesKillswitched || _clientLocProvider == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public void DisplayAcceptChallengeErrors(PVPChallengeData challenge)
	{
		if (challenge != null)
		{
			if (ChallengePermissionState == ChallengeDataProvider.ChallengePermissionState.Restricted_InMatch)
			{
				_systemMessageManager.ShowOk(_clientLocProvider.GetLocalizedText("MainNav/FriendChallenge/CannotAcceptFriendChallengeWhileInMatch_Header"), _clientLocProvider.GetLocalizedText("MainNav/FriendChallenge/CannotAcceptFriendChallengeWhileInMatch_Desc"));
			}
			else if (ChallengePermissionState == ChallengeDataProvider.ChallengePermissionState.Restricted_ChallengesKillswitched)
			{
				_systemMessageManager.ShowOk(_clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeErrorTitle"), _clientLocProvider.GetLocalizedText("MainNav/Challenges/ChallengeErrorBody"));
			}
			else if (GetAllChallenges().Exists((KeyValuePair<Guid, PVPChallengeData> otherChallenge) => otherChallenge.Value.ChallengePlayers.ContainsKey(challenge.LocalPlayerId)) && _clientLocProvider != null)
			{
				SystemMessageManager.ShowSystemMessage(_clientLocProvider.GetLocalizedText("MainNav/FriendChallenge/CannotAcceptFriendChallengeWhileInMatch_Header"), _clientLocProvider.GetLocalizedText("MainNav/FriendChallenge/CannotAcceptFriendChallengeWhileInOtherLobby_Desc"));
			}
		}
	}

	public Promise<PVPChallengeData> CreateAndCacheChallenge(bool forceCreate = false)
	{
		SimplePromise<PVPChallengeData> promise = new SimplePromise<PVPChallengeData>();
		PVPChallengeData activeCurrentChallengeData = GetActiveCurrentChallengeData();
		if (!forceCreate && activeCurrentChallengeData != null)
		{
			promise.SetResult(activeCurrentChallengeData);
			return promise;
		}
		string deckId = MDNPlayerPrefs.GetSelectedDeckId(_accountClient.AccountInformation?.PersonaID, "DirectGame");
		string localPlayerDisplayName = _accountClient.AccountInformation.DisplayName;
		string localPlayerId = _accountClient.AccountInformation.PersonaID;
		_challengeCommunicationWrapper.CreateChallenge(GetChallengeTitle(localPlayerDisplayName)).Then(delegate(Promise<PVPChallengeData> challengeDataPromise)
		{
			if (challengeDataPromise.Successful)
			{
				PVPChallengeData result = challengeDataPromise.Result;
				result.Status = ChallengeStatus.Setup;
				result.LocalPlayerDisplayName = localPlayerDisplayName;
				result.LocalPlayerId = localPlayerId;
				result.LocalPlayer.Cosmetics = GetLocalPlayerCosmetics(result);
				if (!string.IsNullOrEmpty(deckId))
				{
					result.LocalPlayer.DeckId = Guid.Parse(deckId);
					result.LocalPlayer.DeckArtId = GetDeckArtId(Guid.Parse(deckId));
					result.LocalPlayer.DeckTileId = GetDeckTileId(Guid.Parse(deckId));
				}
				_challengeDataProvider.SetChallengeData(result);
				if (_biLogger != null)
				{
					BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengeCreated.ToString()), ("ChallengeId", result.ChallengeId.ToString()), ("ChallengeStatus", result.Status.ToString()), ("ChallengeTitle", result.ChallengeTitle), ("ChallengeOwnerId", result.ChallengeOwnerId), ("LocalPlayerId", result.LocalPlayerId), ("LocalPlayerDisplayName", localPlayerDisplayName), ("StartingPlayer", result.StartingPlayer.ToString()), ("MatchType", result.MatchType.ToString()), ("IsBestOf3", result.IsBestOf3.ToString()), ("IsChallengeFull", result.IsChallengeFull.ToString()));
				}
				promise.SetResult(result);
			}
			else
			{
				SimpleLog.LogError("PVPChallengeController: Failed to create challenge");
				promise.SetError(challengeDataPromise.Error);
			}
		});
		return promise;
	}

	private uint GetDeckArtId(Guid deckId)
	{
		if (_deckDataProvider != null)
		{
			Client_Deck deckForId = _deckDataProvider.GetDeckForId(deckId);
			if (deckForId != null && deckForId.Summary != null)
			{
				return deckForId.Summary.DeckArtId;
			}
		}
		return 0u;
	}

	private uint GetDeckTileId(Guid deckId)
	{
		if (_deckDataProvider != null)
		{
			Client_Deck deckForId = _deckDataProvider.GetDeckForId(deckId);
			if (deckForId != null && deckForId.Summary != null)
			{
				return deckForId.Summary.DeckTileId;
			}
		}
		return 0u;
	}

	private string GetChallengeTitle(string playerName)
	{
		return playerName.Split('#')[0] + "'s Challenge";
	}

	private void RefreshNavBarButtons()
	{
		SceneLoader sceneLoader = SceneLoader.GetSceneLoader();
		if ((object)sceneLoader != null)
		{
			sceneLoader?.GetNavBar()?.RefreshButtons();
		}
	}

	public void HandleLogout()
	{
		RejectAllChallengeInvites();
		CancelAllChallenges();
	}

	public void Dispose()
	{
		IChallengeCommunicationWrapper challengeCommunicationWrapper = _challengeCommunicationWrapper;
		challengeCommunicationWrapper.OnIncomingChallenge = (Action<PVPChallengeData>)Delegate.Remove(challengeCommunicationWrapper.OnIncomingChallenge, new Action<PVPChallengeData>(HandleChallengeInviteUpdate));
		IChallengeCommunicationWrapper challengeCommunicationWrapper2 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper2.OnChallengeResponse = (Action<Guid, string, InviteStatus>)Delegate.Remove(challengeCommunicationWrapper2.OnChallengeResponse, new Action<Guid, string, InviteStatus>(HandleChallengeInviteResponse));
		IChallengeCommunicationWrapper challengeCommunicationWrapper3 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper3.OnChallengeClosedMessage = (Action<Guid>)Delegate.Remove(challengeCommunicationWrapper3.OnChallengeClosedMessage, new Action<Guid>(HandleChallengeClosedMessage));
		IChallengeCommunicationWrapper challengeCommunicationWrapper4 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper4.OnChallengePlayerUpdate = (Action<Guid, ChallengePlayer>)Delegate.Remove(challengeCommunicationWrapper4.OnChallengePlayerUpdate, new Action<Guid, ChallengePlayer>(HandleChallengePlayerUpdate));
		IChallengeCommunicationWrapper challengeCommunicationWrapper5 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper5.OnChallengeGeneralUpdate = (Action<PVPChallengeData>)Delegate.Remove(challengeCommunicationWrapper5.OnChallengeGeneralUpdate, new Action<PVPChallengeData>(HandleChallengeGeneralUpdate));
		IChallengeCommunicationWrapper challengeCommunicationWrapper6 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper6.OnChallengePlayerKickMessage = (Action<Guid>)Delegate.Remove(challengeCommunicationWrapper6.OnChallengePlayerKickMessage, new Action<Guid>(HandleChallengePlayerKickMessage));
		IChallengeCommunicationWrapper challengeCommunicationWrapper7 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper7.OnChallengeCountdownStart = (Action<Guid, int>)Delegate.Remove(challengeCommunicationWrapper7.OnChallengeCountdownStart, new Action<Guid, int>(HandleMatchLaunch));
		IChallengeCommunicationWrapper challengeCommunicationWrapper8 = _challengeCommunicationWrapper;
		challengeCommunicationWrapper8.OnPlayerBlocked = (Action<List<Block>>)Delegate.Remove(challengeCommunicationWrapper8.OnPlayerBlocked, new Action<List<Block>>(OnPlayerBlocked));
		_clientFeatureToggleDataProvider.UnRegisterForToggleUpdates(OnChallengeEnabledChanged);
		if (_connectionManager != null)
		{
			ConnectionManager connectionManager = _connectionManager;
			connectionManager.OnFdReconnected = (Action)Delegate.Remove(connectionManager.OnFdReconnected, new Action(HandleFrontdoorReconnect));
		}
	}

	private void BIMessage_FriendChallengeCanceled(PVPChallengeData challengeData)
	{
		SendBILog(ClientBusinessEventType.DirectGameChallengeCanceled, new DirectGameChallengeCanceled
		{
			EventTime = DateTime.UtcNow,
			DisplayName = challengeData.LocalPlayerDisplayName,
			OpponentDisplayName = challengeData.OpponentFullName
		});
	}

	private void BIMessage_FriendChallengeReceived(PVPChallengeData challengeData)
	{
		SendBILog(ClientBusinessEventType.DirectGameChallengeReceived, new DirectGameChallengeReceived
		{
			EventTime = DateTime.UtcNow,
			DisplayName = challengeData.LocalPlayerDisplayName,
			OpponentDisplayName = challengeData.OpponentFullName,
			DirectGameEventName = challengeData.Mode
		});
	}

	public void BIMessage_FriendChallengeOutcome(PVPChallengeData challengeData, BIChallengeOutcomeState outcome)
	{
		if (_biLogger != null)
		{
			string challengeePlayerId = "";
			string challengerPlayerId = "";
			ISocialManager socialManager = Pantry.Get<ISocialManager>();
			if (socialManager.Friends.Find((SocialEntity f) => f.FullName == challengeData.OpponentFullName)?.PlayerId == null)
			{
				_ = challengeData.OpponentFullName;
			}
			_ = socialManager.LocalPlayer.PlayerId;
			SendBILog(ClientBusinessEventType.SocialFriendChallengeOutcome, new SocialFriendChallengeOutcome
			{
				EventTime = DateTime.UtcNow,
				ChallengeePlayerId = challengeePlayerId,
				ChallengerPlayerId = challengerPlayerId,
				ChallengeOutcome = outcome.ToString()
			});
		}
	}

	private void SendBILog(ClientBusinessEventType businessType, IClientBusinessEventReq payload)
	{
		_biLogger?.Send(businessType, payload);
	}
}
