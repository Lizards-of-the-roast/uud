using System;
using HasbroGo.Social.Models;
using Wizards.Mtga;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Loc;

namespace MTGA.Social;

public class SocialEntity
{
	private PresenceStatus _status;

	public bool HasUnseenMessages;

	public bool HasChatHistory;

	public string FullName { get; private set; }

	public string DisplayName { get; private set; }

	public string NameHash { get; private set; }

	public string SocialId { get; private set; }

	public string PlayerId { get; private set; }

	public PresenceStatus Status
	{
		get
		{
			return _status;
		}
		private set
		{
			_status = value;
			this.OnStatusChanged?.Invoke(value);
		}
	}

	public bool IsOnline => Status != PresenceStatus.Offline;

	public bool IsBlocked { get; private set; }

	public bool IsChattable => Status == PresenceStatus.Available;

	public bool IsCurrentOpponent { get; private set; }

	public int SortOrder { get; private set; }

	public event Action<PresenceStatus> OnStatusChanged;

	public SocialEntity(string fullName)
	{
		SetName(fullName);
		SetPresence(PresenceStatus.Offline);
	}

	public SocialEntity(string fullName, bool isOpponent, bool isChattable, bool isBlocked)
	{
		SetName(fullName);
		SetAsOpponent(isOpponent);
		SetAsBlocked(isBlocked);
		SetPresence((isOpponent && isChattable && !isBlocked) ? PresenceStatus.Available : PresenceStatus.Offline);
	}

	public SocialEntity(FriendWithPresence platformFriend)
	{
		if (platformFriend.Friend.DisplayName != null)
		{
			SetName(platformFriend.Friend.DisplayName.ToString());
		}
		PlayerId = platformFriend.Presence?.PersonaId;
		SocialId = platformFriend.Presence?.AccountId;
		SetPresence(platformFriend.Presence);
	}

	public void UpdateIds(Presence presence, AccountInformation accountInformation)
	{
		if (presence != null)
		{
			PlayerId = presence.PersonaId;
			SocialId = presence.AccountId;
		}
		if (accountInformation != null)
		{
			SetName(accountInformation.DisplayName);
		}
	}

	public SocialEntity(Friend friend)
	{
		if (friend.DisplayName != null)
		{
			SetName(friend.DisplayName.ToString());
		}
		SocialId = friend.AccountId;
		if (!string.IsNullOrEmpty(friend.PersonaId))
		{
			PlayerId = friend.PersonaId;
		}
	}

	public SocialEntity(Presence presence, string displayName)
	{
		SetName(displayName);
		PlayerId = presence.PersonaId;
		SocialId = presence.AccountId;
		SetPresence(presence);
	}

	public SocialEntity(IncomingFriendInvite invite)
	{
		SetName(SocialUtilities.GetInvitePlayerName(invite), displayFullName: true);
		PlayerId = ((!string.IsNullOrEmpty(invite.AccountId)) ? invite.AccountId : DisplayName);
		SocialId = invite.AccountId;
	}

	public SocialEntity(OutgoingFriendInvite invite)
	{
		SetName(SocialUtilities.GetInvitePlayerName(invite), displayFullName: true);
		PlayerId = invite.ReceiverAccountId ?? DisplayName;
		SocialId = invite.ReceiverAccountId;
	}

	public SocialEntity(BlockedUser platformBlock)
	{
		SocialId = platformBlock.AccountId;
		if (platformBlock.DisplayName != null)
		{
			SetName(platformBlock.DisplayName.ToString(), displayFullName: true);
			PlayerId = platformBlock.PersonaId;
		}
	}

	public SocialEntity(string fullDisplayName, string socialId, string playerId = "")
	{
		SetName(fullDisplayName);
		SetPresence(PresenceStatus.Offline);
		SocialId = socialId;
		PlayerId = playerId;
	}

	public void SetPresence(Presence presence)
	{
		if (presence.PlatformStatus == PlatformStatus.Offline)
		{
			SetPresence(PresenceStatus.Offline);
		}
		else if (presence.PlatformStatus == PlatformStatus.Busy)
		{
			SetPresence(PresenceStatus.Busy);
		}
		else
		{
			SetPresence(PresenceStatus.Available);
		}
	}

	public void SetPresence(PresenceStatus status)
	{
		if (IsBlocked)
		{
			Status = PresenceStatus.Offline;
		}
		else
		{
			Status = status;
		}
	}

	public static string PresenceLocalizationKey(PresenceStatus presenceStatus)
	{
		return presenceStatus switch
		{
			PresenceStatus.Offline => "Social/Friends/ConnectionState/Offline", 
			PresenceStatus.Away => "Social/Friends/ConnectionState/Away", 
			PresenceStatus.Busy => "Social/Friends/ConnectionState/Busy", 
			PresenceStatus.Available => "Social/Friends/ConnectionState/Online", 
			_ => "MainNav/General/ErrorTitle", 
		};
	}

	public static string PresenceLocalizedString(PresenceStatus presenceStatus)
	{
		IClientLocProvider clientLocProvider = Pantry.Get<IClientLocProvider>();
		string key = PresenceLocalizationKey(presenceStatus);
		return clientLocProvider.GetLocalizedText(key);
	}

	public void SetPresence(PresenceStatusId status)
	{
		SetPresence(status switch
		{
			PresenceStatusId.Online => PresenceStatus.Available, 
			PresenceStatusId.Busy => PresenceStatus.Busy, 
			_ => PresenceStatus.Offline, 
		});
	}

	public void SetAsBlocked(bool isBlocked)
	{
		IsBlocked = isBlocked;
		if (IsBlocked)
		{
			SetPresence(PresenceStatus.Offline);
		}
	}

	public void SetAsOpponent(bool isOpponent)
	{
		IsCurrentOpponent = isOpponent;
		SetPresence(isOpponent ? PresenceStatus.Available : PresenceStatus.Offline);
	}

	public void UpdateSortOrder()
	{
		if (IsCurrentOpponent)
		{
			SortOrder = 150;
		}
		else if (HasUnseenMessages)
		{
			SortOrder = 50;
		}
		else
		{
			SortOrder = (int)Status;
		}
	}

	public override bool Equals(object obj)
	{
		return FullName == (obj as SocialEntity)?.FullName;
	}

	public override int GetHashCode()
	{
		return FullName.GetHashCode();
	}

	private void SetName(string fullName, bool displayFullName = false)
	{
		FullName = fullName;
		int num = fullName?.LastIndexOf("#", StringComparison.Ordinal) ?? (-1);
		if (num == -1 || displayFullName)
		{
			num = fullName?.Length ?? 0;
		}
		DisplayName = fullName?.Substring(0, num);
		NameHash = fullName?.Substring(num);
	}
}
