using System.Collections.Generic;

namespace Wizards.Mtga.PlayBlade;

public class PlayBladeRecentlyPlayedData
{
	public List<EventAndDeck> EventsAndDecks;

	public PlayBladeRecentlyPlayedData()
	{
		EventsAndDecks = new List<EventAndDeck>();
	}
}
