using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ManaProducedValidation : ISequenceValidator
{
	public bool ValidateSequence(in int idx, ref List<UXEvent> events, out uint length)
	{
		if (events[idx] is ManaProducedUXEvent manaProducedUXEvent)
		{
			length = 1u;
			for (int i = idx + 1; i < events.Count && events[i] is ManaProducedUXEvent manaProducedUXEvent2; i++)
			{
				if (manaProducedUXEvent2._sinkId != manaProducedUXEvent._sinkId)
				{
					break;
				}
				length++;
			}
			return true;
		}
		length = 0u;
		return false;
	}

	bool ISequenceValidator.ValidateSequence(in int startIdx, ref List<UXEvent> events, out uint length)
	{
		return ValidateSequence(in startIdx, ref events, out length);
	}
}
