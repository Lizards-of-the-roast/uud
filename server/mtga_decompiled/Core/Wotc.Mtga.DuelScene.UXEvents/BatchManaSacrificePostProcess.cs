using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class BatchManaSacrificePostProcess : IUXEventGrouper
{
	private readonly ISequenceValidator _sequenceValidator;

	public BatchManaSacrificePostProcess(ISequenceValidator sequenceValidator)
	{
		_sequenceValidator = sequenceValidator ?? NullSequenceValidator.Default;
	}

	public void GroupEvents(in int evtIdx, ref List<UXEvent> events)
	{
		List<UXEventGroup> evtGroups = GenerateEventGroups(evtIdx, ref events);
		if (evtGroups == null)
		{
			return;
		}
		if (evtGroups.Count == 1)
		{
			events.Insert(evtIdx, evtGroups[0]);
		}
		else if (evtGroups.Count > 1)
		{
			List<UXEvent> list = new List<UXEvent>();
			for (int i = 0; i < evtGroups.Count; i++)
			{
				if (i > 0)
				{
					list.Add(new WaitForSecondsUXEvent(0.1f));
				}
				list.Add(evtGroups[i]);
			}
			events.Insert(evtIdx, new UXEventGroup(list));
		}
		WaitUntilUXEvent item = new WaitUntilUXEvent(() => evtGroups.TrueForAll((UXEventGroup x) => x.IsComplete));
		events.Insert(evtIdx + 1, item);
	}

	private List<UXEventGroup> GenerateEventGroups(int idx, ref List<UXEvent> events)
	{
		List<UXEventGroup> list = null;
		int evtIdx = idx;
		while (evtIdx < events.Count)
		{
			UXEventGroup uXEventGroup = GenerateEventGroup(in evtIdx, ref events);
			if (uXEventGroup == null)
			{
				break;
			}
			if (list == null)
			{
				list = new List<UXEventGroup>();
			}
			list.Add(uXEventGroup);
		}
		return list;
	}

	private UXEventGroup GenerateEventGroup(in int evtIdx, ref List<UXEvent> events)
	{
		if (_sequenceValidator.ValidateSequence(in evtIdx, ref events, out var length))
		{
			UXEvent[] array = new UXEvent[length];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = events[i + evtIdx];
			}
			events.RemoveRange(evtIdx, (int)length);
			return new UXEventGroup(array);
		}
		return null;
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
