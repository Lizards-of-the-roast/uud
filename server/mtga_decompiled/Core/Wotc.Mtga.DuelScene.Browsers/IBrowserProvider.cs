namespace Wotc.Mtga.DuelScene.Browsers;

public interface IBrowserProvider
{
	BrowserBase CurrentBrowser { get; }

	bool IsAnyBrowserOpen => CurrentBrowser != null;

	bool IsBrowserVisible
	{
		get
		{
			if (IsAnyBrowserOpen)
			{
				return CurrentBrowser.IsVisible;
			}
			return false;
		}
	}
}
