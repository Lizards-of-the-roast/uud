namespace Wotc.Mtga.DuelScene.Browsers;

public class NullBrowserProvider : IBrowserProvider
{
	public static readonly IBrowserProvider Default = new NullBrowserProvider();

	public BrowserBase CurrentBrowser => null;
}
