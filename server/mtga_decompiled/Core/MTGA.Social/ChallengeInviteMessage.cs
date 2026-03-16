using System;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;

namespace MTGA.Social;

public class ChallengeInviteMessage
{
	public Guid ChallengeId;

	public string ChallengeTitle;

	public ChallengePlayer Sender;

	public ChallengePlayer Recipient;

	public InviteStatus InviteStatus;

	public DateTime InviteSentTime;

	public ChallengeMatchTypes ChallengeType;

	public bool IsBestofThree;
}
