using System;
using Wotc;

namespace Wizards.Mtga.PlayBlade;

public class EnterMatchMakingSignalArgs : SignalArgs
{
	public string InternalEventName { get; private set; }

	public Guid DeckId { get; private set; }

	public EnterMatchMakingSignalArgs(object dispatcher, string eventName, Guid deckId)
		: base(dispatcher)
	{
		InternalEventName = eventName;
		DeckId = deckId;
	}
}
