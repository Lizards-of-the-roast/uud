using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ResolutionEventReordering : IUXEventGrouper
{
	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		for (int i = startIdx; i < events.Count; i++)
		{
			if (!(events[i] is ResolutionEventStartedUXEvent))
			{
				continue;
			}
			int num = events.FindIndex(i + 1, (UXEvent x) => x is ResolutionEventEndedUXEvent);
			if (num != -1)
			{
				UXEvent item = events[num];
				events.RemoveAt(num);
				int num2 = FindIndexOfCreateAbilityOutsideOfResolutionFrame(events, num);
				if (num2 >= 0)
				{
					events.Insert(num2, item);
				}
				else
				{
					events.Add(item);
				}
			}
		}
	}

	private static int FindIndexOfCreateAbilityOutsideOfResolutionFrame(IReadOnlyList<UXEvent> eventList, int frameEndIdx)
	{
		for (int i = frameEndIdx; i < eventList.Count; i++)
		{
			if (eventList[i] is ZoneTransferUXEvent { Reason: ZoneTransferReason.CardCreated, FromZoneType: ZoneType.None, ToZoneType: ZoneType.Stack } zoneTransferUXEvent && zoneTransferUXEvent.OldId == zoneTransferUXEvent.NewId)
			{
				return i;
			}
		}
		return -1;
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
