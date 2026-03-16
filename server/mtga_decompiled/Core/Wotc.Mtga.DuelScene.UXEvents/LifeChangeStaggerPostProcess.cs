using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class LifeChangeStaggerPostProcess : IUXEventGrouper
{
	private const float STAGGER_DURATION = 0.2f;

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		for (int i = startIdx; i < events.Count; i++)
		{
			if (ShouldInsertStagger(i, events))
			{
				events.Insert(i + 1, new WaitForSecondsUXEvent(0.2f));
				i++;
			}
		}
	}

	private static bool ShouldInsertStagger(int idx, IReadOnlyList<UXEvent> events)
	{
		if (!(events[idx] is LifeTotalUpdateUXEvent lifeTotalUpdateUXEvent))
		{
			return false;
		}
		for (int i = idx + 1; i < events.Count; i++)
		{
			UXEvent uXEvent = events[i];
			if (uXEvent.IsBlocking)
			{
				return false;
			}
			if (uXEvent is LifeTotalUpdateUXEvent lifeTotalUpdateUXEvent2)
			{
				return lifeTotalUpdateUXEvent.AffectedId == lifeTotalUpdateUXEvent2.AffectedId;
			}
		}
		return false;
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
