using System;
using System.Collections.Generic;

public interface IViewDismissBrowserProvider : ICardBrowserProvider, IDuelSceneBrowserProvider
{
	string Header { get; }

	string SubHeader { get; }

	Action<DuelScene_CDC> OnCardSelected { get; }

	GREPlayerNum PlayerNum { get; }

	List<DuelScene_CDC> GetCardsToDisplay();
}
