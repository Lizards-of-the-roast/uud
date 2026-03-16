using System;

namespace Wizards.Mtga.PlayBlade;

public readonly struct RecentGamesData
{
	public readonly string EventName;

	public readonly Guid DeckId;

	public RecentGamesData(string eventName, Guid deckId)
	{
		EventName = eventName;
		DeckId = deckId;
	}
}
