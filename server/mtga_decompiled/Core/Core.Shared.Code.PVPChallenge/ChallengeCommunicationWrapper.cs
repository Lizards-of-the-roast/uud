using System;
using System.Collections.Generic;
using System.Linq;
using Core.BI;
using Core.Code.Promises;
using HasbroGo.Social.Models;
using MTGA.Social;
using Newtonsoft.Json;
using SharedClientCore.SharedClientCore.Code.PVPChallenge;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using Wizards.Arena.Enums.Match;
using Wizards.Arena.Models.Network;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wizards.Mtga;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Network.ServiceWrappers;
using Wotc.Mtga.Providers;

namespace Core.Shared.Code.PVPChallenge;

public class ChallengeCommunicationWrapper : IChallengeCommunicationWrapper, IDisposable
{
	public const string CHALLENGE_COMMAND_PREFIX = "\u009b\u0080\u0099\u0091\u0092";

	private Lazy<ChallengeMessageConverter> _messageConverter = new Lazy<ChallengeMessageConverter>();

	private Matchmaking _matchmaking;

	private ISocialManager _socialManager;

	private IAccountClient _accountClient;

	private IChallengeServiceWrapper _challengeService;

	private CosmeticsProvider _cosmeticsProvider;

	public Action<PVPChallengeData> OnIncomingChallenge { get; set; }

	public Action<PVPChallengeData> OnChallengeGeneralUpdate { get; set; }

	public Action<Guid> OnChallengeClosedMessage { get; set; }

	public Action<Guid, string, InviteStatus> OnChallengeResponse { get; set; }

	public Action<Guid, SharedClientCore.SharedClientCore.Code.PVPChallenge.Models.ChallengePlayer> OnChallengePlayerUpdate { get; set; }

	public Action<List<Block>> OnPlayerBlocked { get; set; }

	public Action<Guid> OnChallengePlayerKickMessage { get; set; }

	public Action<Guid, int> OnChallengeCountdownStart { get; set; }

	public ChallengeCommunicationWrapper(Matchmaking matchmaking, ISocialManager socialManager, IAccountClient accountClient, IChallengeServiceWrapper challengeServiceWrapper, CosmeticsProvider cosmeticsProvider)
	{
		_matchmaking = matchmaking;
		_socialManager = socialManager;
		_socialManager.OnGameMessage += HandleIncomingSocialGameMessage;
		_socialManager.BlocksChanged += HandlePlayerBlocked;
		_accountClient = accountClient;
		_challengeService = challengeServiceWrapper;
		_cosmeticsProvider = cosmeticsProvider;
		_challengeService.OnChallengeNotification += HandleChallengeNotification;
	}

	public Promise<PVPChallengeData> IssueChallenge(PVPChallengeData challengeData)
	{
		SimplePromise<PVPChallengeData> promise = new SimplePromise<PVPChallengeData>();
		_challengeService.ChallengeIssue(challengeData.ChallengeId.ToString(), challengeData.MatchType.ToString(), challengeData.LocalPlayer.DeckId.ToString()).Then(delegate(Promise<ChallengeStatusResp> response)
		{
			if (response.Successful)
			{
				promise.SetResult(ConvertToClientModel(response.Result.Challenge));
				BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengePlayerMatchStarted.ToString()), ("ChallengeId", challengeData.ChallengeId.ToString()), ("MatchLaunchCountdown", challengeData.MatchLaunchCountdown.ToString()), ("MatchLaunchDateTime", challengeData.MatchLaunchDateTime.ToString()), ("MatchType", challengeData.MatchType.ToString()), ("IsBestOf3", challengeData.IsBestOf3.ToString()));
			}
			else
			{
				promise.SetError(promise.Error);
			}
		});
		return promise;
	}

	public void CloseChallenge(PVPChallengeData challengeData)
	{
		_challengeService.ChallengeClose(challengeData.ChallengeId.ToString()).ThenOnMainThreadIfError(delegate(Error result)
		{
			if (result.Code == 103 && OnChallengeClosedMessage != null)
			{
				OnChallengeClosedMessage(challengeData.ChallengeId);
			}
		});
	}

	public Promise<PVPChallengeData> CreateChallenge(string name)
	{
		SimplePromise<PVPChallengeData> promise = new SimplePromise<PVPChallengeData>();
		_challengeService.ChallengeCreate(name, "").Then(delegate(Promise<ChallengeStatusResp> response)
		{
			if (response.Successful)
			{
				promise.SetResult(ConvertToClientModel(response.Result.Challenge));
			}
			else
			{
				promise.SetError(promise.Error);
			}
		});
		return promise;
	}

	private void HandleChallengeNotification(ChallengeNotification notification)
	{
		switch (notification.ChallengeNotificationType)
		{
		case EChallengeNotificationType.ChallengeClose:
			if (OnChallengeClosedMessage != null)
			{
				OnChallengeClosedMessage(Guid.Parse(notification.ChallengeId));
			}
			break;
		case EChallengeNotificationType.ChallengeStatus:
			if (OnChallengeGeneralUpdate != null)
			{
				OnChallengeGeneralUpdate(ConvertToClientModel(notification.Challenge));
			}
			break;
		case EChallengeNotificationType.ChallengeStartLaunchCountdown:
			if (OnChallengeCountdownStart != null)
			{
				OnChallengeCountdownStart(Guid.Parse(notification.ChallengeId), notification.CountdownSeconds);
			}
			break;
		case EChallengeNotificationType.ChallengeKick:
			if (OnChallengePlayerKickMessage != null)
			{
				OnChallengePlayerKickMessage(Guid.Parse(notification.ChallengeId));
			}
			break;
		case EChallengeNotificationType.ChallengeChatMessage:
		case EChallengeNotificationType.ChallengeInvite:
			break;
		}
	}

	public void JoinChallengeMatch(EventContext context, PVPChallengeData challengeData)
	{
		_matchmaking.JoinChallenge(context, challengeData);
	}

	public Promise<bool> UpdateChallengeInvite(PVPChallengeData challenge, ChallengeInvite invite)
	{
		ChallengeInviteMessage payload = new ChallengeInviteMessage
		{
			ChallengeId = challenge.ChallengeId,
			ChallengeTitle = challenge.ChallengeTitle,
			Sender = invite.Sender,
			Recipient = invite.Recipient,
			InviteStatus = invite.Status,
			InviteSentTime = invite.InviteSentTime,
			IsBestofThree = challenge.IsBestOf3,
			ChallengeType = challenge.MatchType
		};
		return SendSocialSDKGameMessage(invite.Recipient.PlayerId, ChallengeMessageType.IncomingInvite, payload, "Failed to cancel challenge invite");
	}

	public Promise<PVPChallengeData> RespondToChallengeInvite(Guid challengeId, ChallengeInvite invite)
	{
		SimplePromise<PVPChallengeData> promise = new SimplePromise<PVPChallengeData>();
		if (invite.Status == InviteStatus.Accepted)
		{
			_challengeService.ChallengeJoin(challengeId.ToString()).Then(delegate(Promise<ChallengeStatusResp> joinPromise)
			{
				if (joinPromise.Successful)
				{
					promise.SetResult(ConvertToClientModel(joinPromise.Result.Challenge));
				}
				else
				{
					promise.SetError(joinPromise.Error);
				}
			});
		}
		else if (invite.Status == InviteStatus.Rejected)
		{
			ChallengeInviteResponseMessage challengeInviteResponseMessage = new ChallengeInviteResponseMessage
			{
				ChallengeId = challengeId,
				Sender = invite.Sender,
				Recipient = invite.Recipient,
				InviteStatus = invite.Status
			};
			SendSocialSDKGameMessage(invite.Sender.PlayerId, ChallengeMessageType.RespondToChallenge, challengeInviteResponseMessage, "Failed to cancel challenge invite");
			OnChallengeResponse(challengeInviteResponseMessage.ChallengeId, challengeInviteResponseMessage.Recipient.PlayerId, challengeInviteResponseMessage.InviteStatus);
			promise.SetResult(null);
		}
		else
		{
			promise.SetError(new Error(0, $"Invite status can be sent, only the sender can set status to {invite.Status}"));
		}
		return promise;
	}

	public void LaunchChallenge(PVPChallengeData challengeData)
	{
		_challengeService.ChallengeLaunch(challengeData.ChallengeId.ToString());
	}

	public void BlockPlayer(PVPChallengeData challengeData, string playerId)
	{
		_socialManager.BlockByDisplayName(playerId);
	}

	private void HandlePlayerBlocked()
	{
		OnPlayerBlocked(_socialManager.Blocks);
	}

	public Promise<PVPChallengeData> LeaveChallenge(PVPChallengeData challengeData)
	{
		SimplePromise<PVPChallengeData> promise = new SimplePromise<PVPChallengeData>();
		_challengeService.ChallengeExit(challengeData.ChallengeId.ToString()).Then(delegate(Promise<ChallengeStatusResp> exitPromise)
		{
			if (exitPromise.Successful)
			{
				promise.SetResult(ConvertToClientModel(exitPromise.Result.Challenge));
			}
			else
			{
				promise.SetError(exitPromise.Error);
			}
		});
		return promise;
	}

	public Promise<PVPChallengeData> KickPlayer(PVPChallengeData challengeData, SharedClientCore.SharedClientCore.Code.PVPChallenge.Models.ChallengePlayer kickPlayer)
	{
		SimplePromise<PVPChallengeData> promise = new SimplePromise<PVPChallengeData>();
		_challengeService.ChallengeKick(challengeData.ChallengeId.ToString(), kickPlayer.PlayerId).Then(delegate(Promise<ChallengeStatusResp> kickPromise)
		{
			if (kickPromise.Successful)
			{
				promise.SetResult(ConvertToClientModel(kickPromise.Result.Challenge));
			}
			else
			{
				promise.SetError(kickPromise.Error);
			}
		});
		return promise;
	}

	public Promise<PVPChallengeData> SetGameSettings(string challengeId, ChallengeMatchTypes matchType, WhoPlaysFirst playFirst, bool isBestOf3)
	{
		SimplePromise<PVPChallengeData> promise = new SimplePromise<PVPChallengeData>();
		_challengeService.ChallengeSetSettings(challengeId, matchType.ToString(), GetPlayFirstFromWhoPlaysFirst(playFirst), (!isBestOf3) ? MatchWinCondition.SingleElimination : MatchWinCondition.BestOf3).Then(delegate(Promise<ChallengeStatusResp> setSettingPromise)
		{
			if (setSettingPromise.Successful)
			{
				promise.SetResult(ConvertToClientModel(setSettingPromise.Result.Challenge));
			}
			else
			{
				promise.SetError(setSettingPromise.Error);
			}
		});
		return promise;
	}

	public Promise<List<PVPChallengeData>> ChallengeReconnectAll()
	{
		SimplePromise<List<PVPChallengeData>> ret = new SimplePromise<List<PVPChallengeData>>();
		_challengeService.ChallengeReconnectAll().ThenOnMainThread(delegate(Promise<ChallengeReconnectAllResp> promise)
		{
			if (promise.Successful)
			{
				List<PVPChallengeData> list = new List<PVPChallengeData>();
				foreach (Challenge item in promise.Result.Challenge)
				{
					list.Add(ConvertToClientModel(item));
				}
				ret.SetResult(list);
			}
			else
			{
				ret.SetError(promise.Error);
			}
		});
		return ret;
	}

	public void SendGeneralChallengeUpdate(PVPChallengeData challengeData)
	{
		ChallengeConfigData payload = new ChallengeConfigData
		{
			ChallengeId = challengeData.ChallengeId,
			BestOfId = ((!challengeData.IsBestOf3) ? MatchWinCondition.SingleElimination : MatchWinCondition.BestOf3),
			ChallengeMode = challengeData.Mode,
			ChallengeStatus = challengeData.Status,
			FirstToPlay = Enum.GetName(typeof(WhoPlaysFirst), challengeData.StartingPlayer),
			ChallengePlayers = challengeData.ChallengePlayers,
			Invites = challengeData.Invites,
			MatchStartCountdown = challengeData.MatchLaunchCountdown,
			ChallengeOwnerId = challengeData.ChallengeOwnerId
		};
		foreach (KeyValuePair<string, SharedClientCore.SharedClientCore.Code.PVPChallenge.Models.ChallengePlayer> challengePlayer in challengeData.ChallengePlayers)
		{
			if (challengePlayer.Key != challengeData.LocalPlayerId)
			{
				SendSocialSDKGameMessage(challengePlayer.Key, ChallengeMessageType.GeneralUpdate, payload, "Failed to send general challenge update");
			}
		}
	}

	public Promise<PVPChallengeData> SendPlayerUpdate(PVPChallengeData challengeData, string playerId, PlayerStatus playerStatus)
	{
		SimplePromise<PVPChallengeData> promise = new SimplePromise<PVPChallengeData>();
		SharedClientCore.SharedClientCore.Code.PVPChallenge.Models.ChallengePlayer challengePlayer = challengeData.ChallengePlayers[playerId];
		switch (playerStatus)
		{
		case PlayerStatus.Ready:
			_challengeService.ChallengeReady(challengeData.ChallengeId.ToString(), challengePlayer.DeckId).Then(delegate(Promise<ChallengeStatusResp> readyPromise)
			{
				if (readyPromise.Successful)
				{
					promise.SetResult(ConvertToClientModel(readyPromise.Result.Challenge));
				}
				else
				{
					promise.SetError(readyPromise.Error);
				}
			});
			break;
		case PlayerStatus.NotReady:
			_challengeService.ChallengeUnready(challengeData.ChallengeId.ToString()).Then(delegate(Promise<ChallengeStatusResp> readyPromise)
			{
				if (readyPromise.Successful)
				{
					promise.SetResult(ConvertToClientModel(readyPromise.Result.Challenge));
				}
				else
				{
					promise.SetError(readyPromise.Error);
				}
			});
			break;
		}
		return promise;
	}

	public Promise<Dictionary<string, string>> GetChallengeableFriendDisplayNames()
	{
		SimplePromise<Dictionary<string, string>> simplePromise = new SimplePromise<Dictionary<string, string>>();
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (SocialEntity friend in _socialManager.Friends)
		{
			dictionary[friend.FullName] = friend.PlayerId;
		}
		simplePromise.SetResult(dictionary);
		return simplePromise;
	}

	public List<SocialEntity> GetFriends()
	{
		return _socialManager.Friends;
	}

	public Promise<string> GetPlayerIdFromFullPlayerName(string fullPlayerName)
	{
		return _socialManager.GetPlayerIdFromFullPlayerName(fullPlayerName);
	}

	private Promise<bool> SendSocialSDKGameMessage(string playerId, ChallengeMessageType method, object payload, string errorLog)
	{
		SimplePromise<bool> simplePromise = new SimplePromise<bool>();
		string text = JsonConvert.SerializeObject(new MTGA.Social.ChallengeMessage
		{
			Method = method,
			Params = payload
		}, _messageConverter.Value);
		_socialManager.SendGameMessage(playerId, "\u009b\u0080\u0099\u0091\u0092" + text);
		simplePromise.SetResult(result: true);
		return simplePromise;
	}

	private void HandleIncomingSocialGameMessage(GameMessage gameMessage)
	{
		if (gameMessage.Payload.StartsWith("\u009b\u0080\u0099\u0091\u0092") && _socialManager.LocalPlayer.PlayerId != gameMessage.SenderPersonaId)
		{
			HandleChallengeMessage(gameMessage.Payload, "Test DisplayName", gameMessage.SenderPersonaId);
		}
	}

	private void HandleChallengeMessage(string message, string senderDisplayName, string channelId)
	{
		string text = message.Substring("\u009b\u0080\u0099\u0091\u0092".Length);
		try
		{
			MTGA.Social.ChallengeMessage challengeMessage = JsonConvert.DeserializeObject<MTGA.Social.ChallengeMessage>(text, new JsonConverter[1] { _messageConverter.Value });
			AccountInformation accountInformation = _accountClient.AccountInformation;
			object obj = challengeMessage.Params;
			if (!(obj is ChallengeInviteMessage challengeInviteMessage))
			{
				if (!(obj is ChallengeInviteResponseMessage challengeInviteResponseMessage))
				{
					if (!(obj is ChallengeConfigData challengeConfigData))
					{
						if (obj is ChallengePlayerUpdateMessage challengePlayerUpdateMessage)
						{
							if (OnChallengePlayerUpdate != null)
							{
								OnChallengePlayerUpdate(challengePlayerUpdateMessage.ChallengeId, challengePlayerUpdateMessage.Player);
							}
						}
						else
						{
							SimpleLog.LogError($"[Social.Challenge] Response method did not match any known method: {challengeMessage.Method}");
						}
					}
					else if (OnChallengeGeneralUpdate != null)
					{
						OnChallengeGeneralUpdate(new PVPChallengeData
						{
							ChallengeId = challengeConfigData.ChallengeId,
							LocalPlayerDisplayName = accountInformation.DisplayName,
							LocalPlayerId = accountInformation.PersonaID,
							IsBestOf3 = (challengeConfigData.BestOfId == MatchWinCondition.BestOf3),
							StartingPlayer = ConvertToComplimentaryPlaysFirst(challengeConfigData.FirstToPlay),
							MatchType = Enum.Parse<ChallengeMatchTypes>(challengeConfigData.ChallengeMode),
							Status = challengeConfigData.ChallengeStatus,
							ChallengePlayers = challengeConfigData.ChallengePlayers,
							Invites = challengeConfigData.Invites,
							MatchLaunchCountdown = challengeConfigData.MatchStartCountdown,
							ChallengeOwnerId = challengeConfigData.ChallengeOwnerId
						});
					}
				}
				else if (OnChallengeResponse != null)
				{
					OnChallengeResponse(challengeInviteResponseMessage.ChallengeId, challengeInviteResponseMessage.Recipient.PlayerId, challengeInviteResponseMessage.InviteStatus);
				}
			}
			else if (OnIncomingChallenge != null && challengeInviteMessage.Recipient.PlayerId == accountInformation.PersonaID)
			{
				OnIncomingChallenge(new PVPChallengeData
				{
					ChallengeId = challengeInviteMessage.ChallengeId,
					ChallengeTitle = challengeInviteMessage.ChallengeTitle,
					Status = ChallengeStatus.None,
					LocalPlayerDisplayName = accountInformation.DisplayName,
					LocalPlayerId = accountInformation.PersonaID,
					Invites = new Dictionary<string, ChallengeInvite> { 
					{
						challengeInviteMessage.Recipient.PlayerId,
						new ChallengeInvite
						{
							Sender = challengeInviteMessage.Sender,
							Recipient = challengeInviteMessage.Recipient,
							Status = challengeInviteMessage.InviteStatus,
							InviteSentTime = challengeInviteMessage.InviteSentTime
						}
					} },
					IsBestOf3 = challengeInviteMessage.IsBestofThree,
					MatchType = challengeInviteMessage.ChallengeType
				});
				BIEventType.ChallengeMessage.SendWithDefaults(("ChallengeAction", BIChallengeAction.ChallengeInviteReceived.ToString()), ("ChallengeId", challengeInviteMessage.ChallengeId.ToString()), ("ChallengePlayerSenderId", challengeInviteMessage.Sender.PlayerId), ("ChallengePlayerRecipientId", challengeInviteMessage.Recipient.PlayerId), ("InviteStatus", challengeInviteMessage.InviteStatus.ToString()), ("InviteSentTime", challengeInviteMessage.InviteSentTime.ToString()));
			}
		}
		catch (Exception ex)
		{
			SimpleLog.LogError("[Social.Challenge] Failed to parse JSON payload: " + text);
			SimpleLog.LogError(ex.Message);
		}
	}

	private static WhoPlaysFirst ConvertToComplimentaryPlaysFirst(string incomingFirstToPlay)
	{
		return Enum.Parse<WhoPlaysFirst>(incomingFirstToPlay) switch
		{
			WhoPlaysFirst.LocalPlayer => WhoPlaysFirst.Opponent, 
			WhoPlaysFirst.Opponent => WhoPlaysFirst.LocalPlayer, 
			_ => WhoPlaysFirst.Random, 
		};
	}

	private WhoPlaysFirst GetWhoPlaysFirstFromPlayFirst(PlayFirst playFirst)
	{
		return playFirst switch
		{
			PlayFirst.Opponent => WhoPlaysFirst.Opponent, 
			PlayFirst.Challenger => WhoPlaysFirst.LocalPlayer, 
			PlayFirst.Random => WhoPlaysFirst.Random, 
			_ => WhoPlaysFirst.Random, 
		};
	}

	private PlayFirst GetPlayFirstFromWhoPlaysFirst(WhoPlaysFirst playFirst)
	{
		return playFirst switch
		{
			WhoPlaysFirst.Opponent => PlayFirst.Opponent, 
			WhoPlaysFirst.LocalPlayer => PlayFirst.Challenger, 
			WhoPlaysFirst.Random => PlayFirst.Random, 
			_ => PlayFirst.Random, 
		};
	}

	private SharedClientCore.SharedClientCore.Code.PVPChallenge.Models.ChallengePlayer GetChallengePlayerFromNetworkModel(Wizards.Arena.Models.Network.ChallengePlayer networkModel)
	{
		return new SharedClientCore.SharedClientCore.Code.PVPChallenge.Models.ChallengePlayer
		{
			PlayerId = networkModel.PlayerId,
			PlayerStatus = ((!networkModel.Ready) ? PlayerStatus.NotReady : PlayerStatus.Ready),
			FullDisplayName = networkModel.DisplayName,
			Cosmetics = GetClientModelFromNetworkModel(networkModel.Cosmetics, _cosmeticsProvider),
			DeckArtId = networkModel.DeckArtId,
			DeckTileId = networkModel.DeckTileId
		};
	}

	private static ClientVanitySelectionsV3 GetClientModelFromNetworkModel(PreferredCosmeticsOnDeck networkModel, CosmeticsProvider cosmeticsProvider)
	{
		ClientVanitySelectionsV3 clientVanitySelectionsV = new ClientVanitySelectionsV3();
		if (!string.IsNullOrEmpty(networkModel.Avatar))
		{
			clientVanitySelectionsV.avatarSelection = networkModel.Avatar;
		}
		if (!string.IsNullOrEmpty(networkModel.Sleeve))
		{
			clientVanitySelectionsV.cardBackSelection = networkModel.Sleeve;
		}
		if (!string.IsNullOrEmpty(networkModel.Title))
		{
			clientVanitySelectionsV.titleSelection = ChallengeDataUtils.GetTitleLocKey(networkModel.Title, cosmeticsProvider);
		}
		if (!string.IsNullOrEmpty(networkModel.Pet))
		{
			string[] array = networkModel.Pet.Split(".");
			clientVanitySelectionsV.petSelection = new ClientPetSelection
			{
				name = array[0],
				variant = array[1]
			};
		}
		if (networkModel.Emotes != null)
		{
			clientVanitySelectionsV.emoteSelections = networkModel.Emotes.ToList();
		}
		return clientVanitySelectionsV;
	}

	private string PetSelectionToString(ClientPetSelection petSelection)
	{
		if (string.IsNullOrEmpty(petSelection?.name))
		{
			return null;
		}
		return petSelection.name + "." + petSelection.variant;
	}

	private PVPChallengeData ConvertToClientModel(Challenge networkModel)
	{
		return new PVPChallengeData
		{
			ChallengeId = Guid.Parse(networkModel.ChallengeId),
			ChallengeTitle = networkModel.ChallengeTitle,
			ChallengeOwnerId = networkModel.OwnerPlayerId,
			IsBestOf3 = (networkModel.WinCondition == MatchWinCondition.BestOf3),
			StartingPlayer = GetWhoPlaysFirstFromPlayFirst(networkModel.PlayFirst),
			ChallengePlayers = networkModel.Players.Select(GetChallengePlayerFromNetworkModel).ToDictionary((SharedClientCore.SharedClientCore.Code.PVPChallenge.Models.ChallengePlayer player) => player.PlayerId),
			MatchType = (ChallengeMatchTypes)Enum.Parse(typeof(ChallengeMatchTypes), networkModel.ChallengeName)
		};
	}

	public void Dispose()
	{
		if (_socialManager != null)
		{
			_socialManager.OnGameMessage -= HandleIncomingSocialGameMessage;
		}
	}
}
