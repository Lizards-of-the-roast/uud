using System;
using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ZoneTransferHangTimePostProcess : IUXEventGrouper
{
	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		ZoneTransferUXEvent zoneTransferUXEvent = null;
		foreach (ZoneTransferUXEvent item in ToStackZoneTransfers(events))
		{
			ZoneTransferUXEvent zoneTransferUXEvent2 = (zoneTransferUXEvent = item);
			zoneTransferUXEvent2.RequiredTimeSpentAtDestination = TimeOnStack(zoneTransferUXEvent2);
		}
		if (zoneTransferUXEvent != null && zoneTransferUXEvent.NewInstance.ObjectType != GameObjectType.Ability)
		{
			zoneTransferUXEvent.RequiredTimeSpentAtDestination = 0f;
		}
	}

	private static float TimeOnStack(ZoneTransferUXEvent zte)
	{
		if (zte.NewInstance.Controller == null || !zte.NewInstance.Controller.IsLocalPlayer)
		{
			return 0.4f;
		}
		return 0.25f;
	}

	private static IEnumerable<ZoneTransferUXEvent> ToStackZoneTransfers(IEnumerable<UXEvent> events)
	{
		foreach (UXEvent item in events ?? Array.Empty<UXEvent>())
		{
			if (item is ZoneTransferUXEvent { ToZoneType: ZoneType.Stack } zoneTransferUXEvent)
			{
				yield return zoneTransferUXEvent;
			}
		}
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
