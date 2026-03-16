using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class RemoveDuplicateAddedAbilityPostProcess : IUXEventGrouper
{
	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		foreach (UXEvent item in DuplicateEvents(events))
		{
			events.Remove(item);
		}
	}

	private static IEnumerable<UXEvent> DuplicateEvents(IReadOnlyList<UXEvent> events)
	{
		for (int i = events.Count - 1; i > 0; i--)
		{
			if (events[i] is AbilityAddedUXEvent abilityAddedUXEvent)
			{
				for (int num = i - 1; num >= 0; num--)
				{
					if (events[num] is AbilityAddedUXEvent evt && EventsAreDuplicates(abilityAddedUXEvent, evt))
					{
						yield return abilityAddedUXEvent;
						break;
					}
				}
			}
		}
	}

	private static bool EventsAreDuplicates(AbilityAddedUXEvent evt1, AbilityAddedUXEvent evt2)
	{
		if (evt1.InstanceId == evt2.InstanceId && evt1.AbilityId == evt2.AbilityId)
		{
			return evt1.AffectorId == evt2.AffectorId;
		}
		return false;
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
