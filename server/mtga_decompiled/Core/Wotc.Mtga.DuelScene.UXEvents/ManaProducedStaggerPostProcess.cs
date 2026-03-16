using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ManaProducedStaggerPostProcess : IUXEventGrouper
{
	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		ManaProducedUXEvent manaProducedUXEvent = LastManaProducedUxEvent(events);
		if (manaProducedUXEvent != null)
		{
			manaProducedUXEvent.Blocking = true;
		}
	}

	public static ManaProducedUXEvent LastManaProducedUxEvent(IReadOnlyList<UXEvent> events)
	{
		for (int num = events.Count - 1; num >= 0; num--)
		{
			if (events[num] is ManaProducedUXEvent result)
			{
				return result;
			}
		}
		return null;
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
