namespace Wotc.Mtga.DuelScene.Browsers;

public class OptionalActionBrowser : CardBrowserBase
{
	private readonly IBasicBrowserProvider _browserProvider;

	public OptionalActionBrowser(BrowserManager browserManager, IDuelSceneBrowserProvider provider, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		base.AllowsHoverInteractions = true;
		_browserProvider = (IBasicBrowserProvider)_duelSceneBrowserProvider;
	}

	protected override void SetupCards()
	{
		cardViews = _browserProvider.GetCardsToDisplay();
		MoveCardViewsToBrowser(cardViews);
	}

	protected override void InitializeUIElements()
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(_browserProvider.GetHeaderText());
		component.SetSubheaderText(_browserProvider.GetSubHeaderText());
		base.InitializeUIElements();
	}
}
