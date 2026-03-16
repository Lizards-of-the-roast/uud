using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ManaProducedGrouper : IUXEventGrouper
{
	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		int num = events.FindIndex(startIdx, (UXEvent evt) => evt is ManaProducedUXEvent);
		if (num <= -1 || !(events[num] is ManaProducedUXEvent manaProducedUXEvent))
		{
			return;
		}
		int num2 = 0;
		for (int num3 = num + 1; num3 < events.Count; num3++)
		{
			if (events[num3] is ManaProducedUXEvent manaProducedUXEvent2)
			{
				if (!manaProducedUXEvent.CanGroupWith(manaProducedUXEvent2))
				{
					break;
				}
				events.RemoveAt(num3);
				events.Insert(num + num2, manaProducedUXEvent2);
				num2++;
			}
		}
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
