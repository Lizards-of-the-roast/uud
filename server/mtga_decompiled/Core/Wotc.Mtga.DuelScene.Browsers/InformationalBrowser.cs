namespace Wotc.Mtga.DuelScene.Browsers;

public class InformationalBrowser : BrowserBase
{
	private readonly InformationalBrowserProvider browserProvider;

	public InformationalBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		browserProvider = (InformationalBrowserProvider)provider;
	}

	protected override void InitializeUIElements()
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(browserProvider.GetHeaderText());
		component.SetSubheaderText(browserProvider.GetSubHeaderText());
		base.InitializeUIElements();
	}
}
