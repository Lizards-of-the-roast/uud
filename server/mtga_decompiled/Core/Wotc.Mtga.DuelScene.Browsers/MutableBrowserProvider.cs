namespace Wotc.Mtga.DuelScene.Browsers;

public class MutableBrowserProvider : IBrowserProvider
{
	public BrowserBase CurrentBrowser { get; set; }
}
