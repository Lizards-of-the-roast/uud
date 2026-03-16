using System.Collections.Generic;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class RemoveResolutionCoinFlips : IUXEventGrouper
{
	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		int num = events.FindIndex((UXEvent x) => x is ResolutionEventStartedUXEvent resolutionEventStartedUXEvent && resolutionEventStartedUXEvent.IgnoreCoinFlipEvents);
		if (num != -1)
		{
			int num2 = events.FindIndex(num + 1, (UXEvent x) => x is ResolutionEventEndedUXEvent);
			num2 = ((num2 == -1) ? events.Count : (num2 - 1));
			events.RemoveRange(num + 1, num2, (UXEvent x) => x is ChooseRandomUXEvent_CoinFlip);
		}
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
