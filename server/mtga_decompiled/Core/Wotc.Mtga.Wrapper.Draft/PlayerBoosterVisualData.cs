namespace Wotc.Mtga.Wrapper.Draft;

public readonly struct PlayerBoosterVisualData
{
	public readonly CollationMapping[] DraftBoosterViews;

	public PlayerBoosterVisualData(CollationMapping[] draftBoosterViews)
	{
		DraftBoosterViews = draftBoosterViews;
	}
}
