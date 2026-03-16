using System;

namespace Wotc.Mtga.DuelScene.Browsers;

public class NullBrowserManager : IBrowserManager, IBrowserProvider, IBrowserController
{
	public static readonly IBrowserManager Default = new NullBrowserManager();

	private static readonly IBrowserProvider _provider = NullBrowserProvider.Default;

	private static readonly IBrowserController _controller = NullBrowserController.Default;

	public BrowserBase CurrentBrowser => _provider.CurrentBrowser;

	public event Action<BrowserBase> BrowserOpened
	{
		add
		{
		}
		remove
		{
		}
	}

	public event Action<BrowserBase> BrowserClosed
	{
		add
		{
		}
		remove
		{
		}
	}

	public IBrowser OpenBrowser(IDuelSceneBrowserProvider browserTypeProvider)
	{
		return _controller.OpenBrowser(browserTypeProvider);
	}

	public void CloseCurrentBrowser()
	{
		_controller.CloseCurrentBrowser();
	}
}
