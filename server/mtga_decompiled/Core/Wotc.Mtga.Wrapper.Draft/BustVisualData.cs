namespace Wotc.Mtga.Wrapper.Draft;

public readonly struct BustVisualData
{
	public readonly string DisplayName;

	public readonly string AvatarId;

	public BustVisualData(string displayName, string avatarId)
	{
		DisplayName = displayName;
		AvatarId = avatarId;
	}
}
