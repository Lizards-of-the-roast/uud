using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class CullSuperfluousResolutionStart : IUXEventGrouper
{
	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		for (int i = 0; i < events.Count; i++)
		{
			if (!(events[i] is ResolutionEventStartedUXEvent))
			{
				continue;
			}
			int num = i + 1;
			while (num < events.Count)
			{
				if (events[num] is ResolutionEventEndedUXEvent)
				{
					i = num;
					break;
				}
				if (events[num] is ResolutionEventStartedUXEvent)
				{
					events.RemoveAt(num);
				}
				else
				{
					num++;
				}
			}
		}
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
