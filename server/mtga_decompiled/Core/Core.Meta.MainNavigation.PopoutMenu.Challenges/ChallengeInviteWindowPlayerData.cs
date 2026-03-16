using System.Collections.Generic;

namespace Core.Meta.MainNavigation.PopoutMenu.Challenges;

public class ChallengeInviteWindowPlayerData
{
	public List<ChallengeInviteWindowPlayer> InvitedPlayers { get; set; }

	public List<ChallengeInviteWindowPlayer> FriendPlayers { get; set; }

	public ChallengeInviteWindowPlayerData()
	{
		InvitedPlayers = new List<ChallengeInviteWindowPlayer>();
		FriendPlayers = new List<ChallengeInviteWindowPlayer>();
	}
}
