using System;
using System.Collections.Generic;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class IndexedPostProcessAggregate : IUXEventGrouper
{
	private IEnumerable<IUXEventGrouper> _elements;

	public IndexedPostProcessAggregate(params IUXEventGrouper[] elements)
	{
		_elements = elements ?? Array.Empty<IUXEventGrouper>();
	}

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		foreach (IUXEventGrouper element in _elements)
		{
			for (int i = 0; i < events.Count; i++)
			{
				element.GroupEvents(in i, ref events);
			}
		}
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
