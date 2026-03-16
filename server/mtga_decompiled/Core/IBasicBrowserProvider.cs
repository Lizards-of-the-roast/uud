using System.Collections.Generic;

public interface IBasicBrowserProvider : ICardBrowserProvider, IDuelSceneBrowserProvider, IBrowserHeaderProvider
{
	List<DuelScene_CDC> GetCardsToDisplay();
}
