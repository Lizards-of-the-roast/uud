namespace Wotc.Mtga.Wrapper.Draft;

public readonly struct StaticDraftStateVisualData
{
	public readonly bool IsInteractable;

	public readonly string LocalSeatDisplayName;

	public readonly string LocalSeatAvatarId;

	public readonly string LeftSeatAvatarId;

	public readonly string RightSeatAvatarId;

	public StaticDraftStateVisualData(bool isInteractable, string localSeatDisplayName, string localSeatAvatarId, string leftSeatAvatarId, string rightSeatAvatarId)
	{
		IsInteractable = isInteractable;
		LocalSeatDisplayName = localSeatDisplayName;
		LocalSeatAvatarId = localSeatAvatarId;
		LeftSeatAvatarId = leftSeatAvatarId;
		RightSeatAvatarId = rightSeatAvatarId;
	}
}
