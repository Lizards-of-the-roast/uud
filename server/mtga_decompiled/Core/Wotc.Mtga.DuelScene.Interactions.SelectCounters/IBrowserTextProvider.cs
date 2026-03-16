namespace Wotc.Mtga.DuelScene.Interactions.SelectCounters;

public interface IBrowserTextProvider
{
	BrowserText GetBrowserText(uint sourceId, uint count);
}
