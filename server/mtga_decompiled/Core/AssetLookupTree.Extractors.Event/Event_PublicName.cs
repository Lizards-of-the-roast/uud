using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Event;

public class Event_PublicName : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.CampaignGraphNodeName ?? bb.Event?.PlayerEvent?.EventUXInfo?.PublicEventName;
		return !string.IsNullOrEmpty(value);
	}
}
