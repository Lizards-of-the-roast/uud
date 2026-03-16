using System;
using Wotc;

namespace Wizards.Mtga.PlayBlade;

public class EditDeckSignalArgs : SignalArgs
{
	public Guid DeckId { get; private set; }

	public string EventId { get; private set; }

	public string EventFormat { get; private set; }

	public bool IsInvalidForFormat { get; private set; }

	public EditDeckSignalArgs(object dispatcher, Guid id, string eventId, string eventFormat, bool isInvalidForFormat)
		: base(dispatcher)
	{
		DeckId = id;
		EventId = eventId;
		EventFormat = eventFormat;
		IsInvalidForFormat = isInvalidForFormat;
	}
}
