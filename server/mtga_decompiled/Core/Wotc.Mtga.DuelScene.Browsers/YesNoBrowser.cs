namespace Wotc.Mtga.DuelScene.Browsers;

public class YesNoBrowser : BrowserBase
{
	private readonly YesNoProvider _provider;

	public YesNoBrowser(YesNoProvider provider, BrowserManager browserManager, GameManager gameManager)
		: base(browserManager, provider, gameManager)
	{
		_provider = provider;
	}

	protected override void InitializeUIElements()
	{
		BrowserHeader component = GetBrowserElement("Header").GetComponent<BrowserHeader>();
		component.SetHeaderText(_provider.Header);
		component.SetSubheaderText(_provider.SubHeader);
		base.InitializeUIElements();
	}

	protected override void OnButtonCallback(string buttonKey)
	{
		if (_provider.ButtonKeyToActionMap.TryGetValue(buttonKey, out var value))
		{
			value?.Invoke();
		}
		Close();
	}
}
