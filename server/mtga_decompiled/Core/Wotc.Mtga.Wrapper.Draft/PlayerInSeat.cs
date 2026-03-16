using Wizards.Unification.Models.Draft;

namespace Wotc.Mtga.Wrapper.Draft;

public readonly struct PlayerInSeat
{
	public readonly string DisplayName;

	public readonly string AvatarId;

	public readonly bool IsReady;

	public PlayerInSeat(string displayName, string avatarId, bool isReady)
	{
		DisplayName = displayName;
		AvatarId = avatarId;
		IsReady = isReady;
	}

	public PlayerInSeat(PlayerInfo playerInfo)
	{
		DisplayName = playerInfo.ScreenName;
		AvatarId = playerInfo.Avatar;
		IsReady = true;
	}
}
