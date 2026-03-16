using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Event;

public class Event_InternalName : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.CampaignGraphNodeName ?? bb.Event?.PlayerEvent?.EventInfo?.InternalEventName;
		return !string.IsNullOrEmpty(value);
	}
}
