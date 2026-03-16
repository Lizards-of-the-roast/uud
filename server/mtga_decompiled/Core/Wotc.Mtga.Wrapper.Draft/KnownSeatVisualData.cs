namespace Wotc.Mtga.Wrapper.Draft;

public readonly struct KnownSeatVisualData
{
	public readonly string DisplayName;

	public readonly string AvatarId;

	public readonly string StatusKey;

	public readonly bool IsReady;

	public KnownSeatVisualData(string username, string avatarId, string statusKey, bool isReady)
	{
		DisplayName = username;
		AvatarId = avatarId;
		StatusKey = statusKey;
		IsReady = isReady;
	}
}
