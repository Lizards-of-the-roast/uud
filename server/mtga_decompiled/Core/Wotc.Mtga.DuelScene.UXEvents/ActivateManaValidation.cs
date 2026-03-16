using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ActivateManaValidation : ISequenceValidator
{
	public bool ValidateSequence(in int idx, ref List<UXEvent> events, out uint length)
	{
		bool flag = IsUserActivatedAction(events[idx]);
		length = (flag ? 1u : 0u);
		return flag;
	}

	private bool IsUserActivatedAction(UXEvent evt)
	{
		if (evt is UserActionTakenUXEvent userActionTakenUXEvent)
		{
			return userActionTakenUXEvent.ActionType == ActionType.ActivateMana;
		}
		return false;
	}

	bool ISequenceValidator.ValidateSequence(in int startIdx, ref List<UXEvent> events, out uint length)
	{
		return ValidateSequence(in startIdx, ref events, out length);
	}
}
