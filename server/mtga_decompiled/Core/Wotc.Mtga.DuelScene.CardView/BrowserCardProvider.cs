using System;
using System.Collections.Generic;
using Wotc.Mtga.DuelScene.Browsers;

namespace Wotc.Mtga.DuelScene.CardView;

public class BrowserCardProvider : IVisualStateCardProvider
{
	private readonly CardHolderManager _cardHolderManager;

	private readonly BrowserManager _browserManager;

	public BrowserCardProvider(CardHolderManager cardHolderManager, BrowserManager browserManager)
	{
		_cardHolderManager = cardHolderManager;
		_browserManager = browserManager;
	}

	public IEnumerable<DuelScene_CDC> GetCardViews()
	{
		return BrowserCardViews(_cardHolderManager, _browserManager);
	}

	private static IEnumerable<DuelScene_CDC> BrowserCardViews(CardHolderManager cardHolderManager, BrowserManager browserManager)
	{
		if (browserManager != null && cardHolderManager != null && browserManager.IsBrowserVisible)
		{
			CardBrowserCardHolder defaultBrowser = cardHolderManager.DefaultBrowser;
			if ((object)defaultBrowser != null)
			{
				return defaultBrowser.CardViews;
			}
		}
		return Array.Empty<DuelScene_CDC>();
	}
}
