using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Event;

public class Event_State : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)(bb.Event?.PlayerEvent?.EventInfo?.EventState).GetValueOrDefault();
		return bb.Event?.PlayerEvent?.EventInfo != null;
	}
}
