using System.Collections.Generic;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class CardTappedValidation : ISequenceValidator
{
	public bool ValidateSequence(in int idx, ref List<UXEvent> events, out uint length)
	{
		bool flag = IsCardTappedEvent(events[idx]);
		length = (flag ? 1u : 0u);
		return flag;
	}

	private bool IsCardTappedEvent(UXEvent evt)
	{
		if (evt is UpdateCardModelUXEvent updateCardModelUXEvent)
		{
			return updateCardModelUXEvent.Property == PropertyType.IsTapped;
		}
		return false;
	}

	bool ISequenceValidator.ValidateSequence(in int startIdx, ref List<UXEvent> events, out uint length)
	{
		return ValidateSequence(in startIdx, ref events, out length);
	}
}
