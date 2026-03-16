namespace Wotc.Mtga.Hangers;

public readonly struct HangerConfig
{
	public readonly string Header;

	public readonly string Details;

	public readonly string Addendum;

	public readonly string SpritePath;

	public readonly bool ConvertSymbols;

	public readonly HangerColor Color;

	public readonly int Section;

	public HangerConfig(string header, string details, string addendum = null, string spritePath = null, bool convertSymbols = true, HangerColor color = HangerColor.None, int section = 0)
	{
		Header = header;
		Details = details;
		Addendum = addendum ?? string.Empty;
		SpritePath = spritePath ?? string.Empty;
		ConvertSymbols = convertSymbols;
		Color = color;
		Section = section;
	}

	public override string ToString()
	{
		string text = "[" + Header + " " + Details;
		if (!ConvertSymbols)
		{
			text += $" (ConvertSymbols: {ConvertSymbols})";
		}
		if (!string.IsNullOrEmpty(Addendum))
		{
			text = text + " (Addendum: " + Addendum + ")";
		}
		if (!string.IsNullOrEmpty(SpritePath))
		{
			text = text + " (Sprite: " + SpritePath + ")";
		}
		if (Color != HangerColor.None)
		{
			text += $" (Color: {Color})";
		}
		if (Section != 0)
		{
			text += $" (Section: {Section})";
		}
		return text + "]";
	}
}
