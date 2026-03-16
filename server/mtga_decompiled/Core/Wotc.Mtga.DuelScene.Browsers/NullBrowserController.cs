namespace Wotc.Mtga.DuelScene.Browsers;

public class NullBrowserController : IBrowserController
{
	public static readonly IBrowserController Default = new NullBrowserController();

	public IBrowser OpenBrowser(IDuelSceneBrowserProvider browserTypeProvider)
	{
		return null;
	}

	public void CloseCurrentBrowser()
	{
	}
}
