using System.Collections.Generic;

public interface IGroupBrowserProvider : ICardBrowserProvider, IDuelSceneBrowserProvider
{
	List<List<DuelScene_CDC>> GetCardsToDisplay();
}
