using System;
using System.Collections.Generic;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;
using Wizards.Arena.Enums.Match;

namespace MTGA.Social;

public class ChallengeConfigData
{
	public Guid ChallengeId;

	public string ChallengeOwnerId;

	public string FirstToPlay;

	public MatchWinCondition BestOfId;

	public string ChallengeMode;

	public ChallengeStatus ChallengeStatus;

	public Dictionary<string, ChallengeInvite> Invites;

	public Dictionary<string, ChallengePlayer> ChallengePlayers;

	public int MatchStartCountdown;
}
