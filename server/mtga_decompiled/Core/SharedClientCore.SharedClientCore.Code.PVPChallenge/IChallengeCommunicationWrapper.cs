using System;
using System.Collections.Generic;
using MTGA.Social;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using Wizards.Arena.Promises;
using Wizards.MDN;
using Wotc.Mtga.Network.ServiceWrappers;

namespace SharedClientCore.SharedClientCore.Code.PVPChallenge;

public interface IChallengeCommunicationWrapper : IDisposable
{
	Action<PVPChallengeData> OnIncomingChallenge { get; set; }

	Action<PVPChallengeData> OnChallengeGeneralUpdate { get; set; }

	Action<Guid> OnChallengeClosedMessage { get; set; }

	Action<Guid, string, InviteStatus> OnChallengeResponse { get; set; }

	Action<Guid, ChallengePlayer> OnChallengePlayerUpdate { get; set; }

	Action<Guid> OnChallengePlayerKickMessage { get; set; }

	Action<Guid, int> OnChallengeCountdownStart { get; set; }

	Action<List<Block>> OnPlayerBlocked { get; set; }

	Promise<PVPChallengeData> CreateChallenge(string name);

	Promise<PVPChallengeData> SetGameSettings(string challengeId, ChallengeMatchTypes matchType, WhoPlaysFirst playFirst, bool isBestOf3);

	Promise<PVPChallengeData> IssueChallenge(PVPChallengeData challengeData);

	void CloseChallenge(PVPChallengeData challengeData);

	void BlockPlayer(PVPChallengeData challengeData, string playerId);

	Promise<PVPChallengeData> KickPlayer(PVPChallengeData challengeData, ChallengePlayer kickPlayer);

	Promise<bool> UpdateChallengeInvite(PVPChallengeData challenge, ChallengeInvite invite);

	Promise<PVPChallengeData> RespondToChallengeInvite(Guid challengeId, ChallengeInvite invite);

	void SendGeneralChallengeUpdate(PVPChallengeData challengeData);

	Promise<PVPChallengeData> SendPlayerUpdate(PVPChallengeData challengeData, string playerId, PlayerStatus playerStatus);

	void LaunchChallenge(PVPChallengeData challengeData);

	Promise<PVPChallengeData> LeaveChallenge(PVPChallengeData challengeData);

	void JoinChallengeMatch(EventContext eventContext, PVPChallengeData challengeData);

	Promise<List<PVPChallengeData>> ChallengeReconnectAll();

	Promise<Dictionary<string, string>> GetChallengeableFriendDisplayNames();

	Promise<string> GetPlayerIdFromFullPlayerName(string fullPlayerName);

	List<SocialEntity> GetFriends();
}
