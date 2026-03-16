using System.Collections.Generic;

public interface ISelectCardsBrowserProvider : IBasicBrowserProvider, ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider
{
	bool AllowKeyboardSelection { get; }

	IEnumerable<DuelScene_CDC> GetSelectableCdcs();

	IEnumerable<DuelScene_CDC> GetNonSelectableCdcs();

	Dictionary<DuelScene_CDC, HighlightType> GetBrowserHighlights();
}
