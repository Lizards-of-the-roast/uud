using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Event;

public class Event_Type : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)(bb.Event?.PlayerEvent?.EventInfo?.FormatType).GetValueOrDefault();
		return bb.Event?.PlayerEvent?.EventInfo != null;
	}
}
