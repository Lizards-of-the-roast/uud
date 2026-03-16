namespace Wotc.Mtga.DuelScene.Browsers;

public interface IBrowserController
{
	IBrowser OpenBrowser(IDuelSceneBrowserProvider browserTypeProvider);

	void CloseCurrentBrowser();
}
