namespace Wotc.Mtga.DuelScene.Interactions.SelectCounters;

public readonly struct BrowserText
{
	public readonly string Header;

	public readonly string SubHeader;

	public BrowserText(string header, string subHeader)
	{
		Header = header ?? string.Empty;
		SubHeader = subHeader ?? string.Empty;
	}
}
