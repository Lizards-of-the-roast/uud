using System;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;

namespace MTGA.Social;

public class ChallengeInviteResponseMessage
{
	public Guid ChallengeId;

	public ChallengePlayer Sender;

	public ChallengePlayer Recipient;

	public InviteStatus InviteStatus;
}
