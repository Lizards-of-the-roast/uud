using System;

namespace Wotc.Mtga.DuelScene.Browsers;

public interface IBrowserManager : IBrowserProvider, IBrowserController
{
	event Action<BrowserBase> BrowserOpened;

	event Action<BrowserBase> BrowserClosed;
}
