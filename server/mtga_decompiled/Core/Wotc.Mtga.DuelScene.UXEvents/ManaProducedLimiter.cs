using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ManaProducedLimiter : IUXEventGrouper
{
	public const int MAX_SEQUENTIAL_MANA_PAID = 5;

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		if (!events.Exists((UXEvent x) => x is ManaProducedUXEvent))
		{
			return;
		}
		for (int num = startIdx; num < events.Count; num++)
		{
			if (!(events[num] is ManaProducedUXEvent manaProducedUXEvent))
			{
				continue;
			}
			int num2 = 1;
			int num3 = num + 1;
			while (num3 < events.Count && manaProducedUXEvent.IsSame(events[num3]))
			{
				if (num2 > 5)
				{
					events.RemoveAt(num3);
				}
				else
				{
					num2++;
				}
				num3++;
				num++;
			}
		}
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
