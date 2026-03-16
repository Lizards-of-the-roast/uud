namespace Wotc.Mtga.DuelScene.Browsers;

public class ScrollListBrowser : BrowserBase
{
	protected BrowserScrollList scrollList;

	public ScrollListBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
	}

	protected override void InitializeUIElements()
	{
		base.InitializeUIElements();
		scrollList = GetBrowserElement("ScrollList").GetComponent<BrowserScrollList>();
	}
}
