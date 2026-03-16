using System;
using HasbroGo.Social.Models;

namespace MTGA.Social;

public class Invite
{
	public string InviteId { get; private set; }

	public Direction Direction { get; private set; }

	public SocialEntity PotentialFriend { get; private set; }

	public string ReceiverEmail { get; private set; }

	public DateTime CreatedAt { get; private set; }

	public Invite(IncomingFriendInvite invite)
	{
		SetPlatformInvite(invite);
	}

	public Invite(OutgoingFriendInvite invite)
	{
		SetPlatformInvite(invite);
	}

	public void SetPlatformInvite(IncomingFriendInvite invite)
	{
		InviteId = invite.InviteId;
		Direction = Direction.Incoming;
		PotentialFriend = new SocialEntity(invite);
		CreatedAt = invite.CreatedAtUtc;
	}

	public void SetPlatformInvite(OutgoingFriendInvite invite)
	{
		InviteId = invite.InviteId;
		Direction = Direction.Outgoing;
		PotentialFriend = new SocialEntity(invite);
		CreatedAt = invite.CreatedAtUtc;
		if (invite.ReceiverEmail != null)
		{
			ReceiverEmail = invite.ReceiverEmail.ToString();
		}
	}
}
