using System.Collections;

namespace Wizards.MDN.DeckManager;

public interface IDeckSleeveProvider
{
	string GetDefaultSleeve();

	IEnumerator Coroutine_UpdateAllDecksWithDefaultSleeve();
}
