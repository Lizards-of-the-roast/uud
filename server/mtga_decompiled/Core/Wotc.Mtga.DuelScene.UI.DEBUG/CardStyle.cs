namespace Wotc.Mtga.DuelScene.UI.DEBUG;

public readonly struct CardStyle
{
	public readonly uint GrpId;

	public readonly string Style;

	public readonly string Description;

	public CardStyle(uint grpId, string style, string description)
	{
		GrpId = grpId;
		Style = style;
		Description = description;
	}
}
