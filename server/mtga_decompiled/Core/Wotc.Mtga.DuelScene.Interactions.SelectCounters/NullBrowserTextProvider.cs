namespace Wotc.Mtga.DuelScene.Interactions.SelectCounters;

public class NullBrowserTextProvider : IBrowserTextProvider
{
	public static readonly IBrowserTextProvider Default = new NullBrowserTextProvider();

	public BrowserText GetBrowserText(uint sourceId, uint count)
	{
		return new BrowserText(string.Empty, string.Empty);
	}
}
