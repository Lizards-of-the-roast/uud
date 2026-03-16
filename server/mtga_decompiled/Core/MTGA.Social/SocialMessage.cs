using System;
using System.Linq;
using SharedClientCore.SharedClientCore.Code.ClientModels;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using Wizards.Arena.Models.Network;

namespace MTGA.Social;

public class SocialMessage
{
	public readonly MessageType Type;

	public readonly MTGALocalizedString TextTitle;

	public readonly MTGALocalizedString TextBody;

	public readonly SocialEntity Friend;

	public readonly Direction Direction;

	public readonly Client_Lobby Lobby;

	public readonly Guid ChallengeId;

	public bool Canceled;

	public MessageSeen Seen;

	public SocialMessage(SocialEntity friend, MTGALocalizedString body, Direction direction)
	{
		Friend = friend;
		Type = MessageType.Chat;
		TextBody = body;
		Direction = direction;
		if (Direction == Direction.Incoming)
		{
			TextTitle = new UnlocalizedMTGAString(Friend?.DisplayName);
		}
	}

	public SocialMessage(PVPChallengeData challengeData, SocialEntity friend, ChallengeInvite invite)
	{
		Type = MessageType.Challenge;
		Friend = friend;
		ChallengeId = challengeData.ChallengeId;
		Direction = ((!(challengeData.LocalPlayerDisplayName == invite.Sender.FullDisplayName)) ? Direction.Incoming : Direction.Outgoing);
		if (Direction == Direction.Incoming)
		{
			TextTitle = new UnlocalizedMTGAString(Friend?.DisplayName);
		}
		if (challengeData.MatchType == ChallengeMatchTypes.DirectGameBrawl)
		{
			TextBody = "Social/Notifications/BrawlChallengeIn";
		}
		else if (challengeData.MatchType == ChallengeMatchTypes.DirectGameTournamentMode || challengeData.MatchType == ChallengeMatchTypes.DirectGameTournamentHistoric || challengeData.MatchType == ChallengeMatchTypes.DirectGameTournamentLimited)
		{
			TextBody = "Social/Notifications/TournamentChallengeIn";
		}
		else if (challengeData.IsBestOf3)
		{
			TextBody = "Social/Notifications/MatchChallengeIn";
		}
		else
		{
			TextBody = "Social/Notifications/GameChallengeIn";
		}
	}

	public SocialMessage(Invite invite)
	{
		Type = MessageType.Invite;
		Friend = invite.PotentialFriend;
		Direction = invite.Direction;
		if (Direction == Direction.Incoming)
		{
			TextTitle = new UnlocalizedMTGAString(Friend?.DisplayName);
		}
		TextBody = "Social/Notifications/FriendInviteIn";
	}

	public SocialMessage(Client_Lobby lobby, LobbyMessage lobbyMessage)
	{
		string text = lobby.Lobby.Players.Where((LobbyPlayer p) => p.PlayerId == lobbyMessage.PlayerId)?.FirstOrDefault()?.DisplayName;
		Type = MessageType.LobbyChat;
		TextTitle = new UnlocalizedMTGAString(lobby.Lobby.LobbyName + ": " + text);
		TextBody = new UnlocalizedMTGAString(lobbyMessage.Message);
		Direction = Direction.Incoming;
		Friend = new SocialEntity(text);
		Lobby = lobby;
	}

	public SocialMessage(Client_LobbyInvite lobbyInvite)
	{
		Type = MessageType.LobbyInvite;
		Friend = new SocialEntity(lobbyInvite.InviterName);
		Direction = Direction.Incoming;
		TextTitle = new UnlocalizedMTGAString(lobbyInvite.LobbyName + ": " + lobbyInvite.InviterName);
		TextBody = "Social/Notifications/FriendInviteIn";
	}

	public SocialMessage(Client_TournamentIsReady tournamentReadyNotification)
	{
		Type = MessageType.TournamentIsReady;
		Friend = new SocialEntity("Tournament");
		Direction = Direction.Incoming;
		TextTitle = new UnlocalizedMTGAString("Tournament is ready");
		TextBody = new UnlocalizedMTGAString("Ready up for tournament: " + tournamentReadyNotification.TournamentId);
	}

	public SocialMessage(Client_TournamentRoundIsReady tournamentRoundReadyNotification)
	{
		Type = MessageType.TournamentRoundIsReady;
		Friend = new SocialEntity("Tournament");
		Direction = Direction.Incoming;
		TextTitle = new UnlocalizedMTGAString("Tournament round is ready");
		TextBody = new UnlocalizedMTGAString("Ready up for tournament. Opponent will be: " + tournamentRoundReadyNotification.OpponentDisplayNames?.FirstOrDefault());
	}
}
