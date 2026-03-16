using System;

namespace Wotc.Mtga.DuelScene.Browsers;

public interface ICardBrowser : IBrowser
{
	DuelSceneBrowserType BrowserType { get; }

	string CardHolderLayoutKey { get; }

	ICardHolder CardHolder { get; }

	event Action<DuelScene_CDC> CardViewSelectedHandlers;

	event Action PreReleaseCardViewsHandlers;

	event Action<DuelScene_CDC> ReleaseCardViewHandlers;

	event Action<DuelScene_CDC> CardViewRemovedHandlers;

	void OnCardViewSelected(DuelScene_CDC cardView);
}
