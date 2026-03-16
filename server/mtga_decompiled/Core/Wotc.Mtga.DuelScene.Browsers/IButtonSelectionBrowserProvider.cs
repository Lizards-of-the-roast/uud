using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.Browsers;

public interface IButtonSelectionBrowserProvider : IDuelSceneBrowserProvider, IBrowserHeaderProvider
{
	Dictionary<string, ButtonStateData> GetScrollListButtonStateData();

	Dictionary<string, ButtonStateData> GetSelectedScrollListButtonStateData();

	bool SortButtonsByKey();
}
