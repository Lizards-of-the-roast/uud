using System;
using SharedClientCore.SharedClientCore.Code.PVPChallenge.Models;

namespace MTGA.Social;

public class ChallengePlayerUpdateMessage
{
	public Guid ChallengeId;

	public ChallengePlayer Player;
}
