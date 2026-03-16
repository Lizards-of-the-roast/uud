using AssetLookupTree.Blackboard;
using Wotc.Mtga.Events;

namespace AssetLookupTree.Extractors.Event;

public class Event_TimerState : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.Event?.PlayerEvent?.CourseData != null && bb.Event?.PlayerEvent?.EventInfo != null)
		{
			value = (int)(bb.Event?.PlayerEvent?.GetTimerState()).GetValueOrDefault();
			return true;
		}
		value = 0;
		return false;
	}
}
