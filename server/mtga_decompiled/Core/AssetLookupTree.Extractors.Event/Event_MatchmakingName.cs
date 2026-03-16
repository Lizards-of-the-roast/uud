using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Event;

public class Event_MatchmakingName : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.Event?.PlayerEvent?.MatchMakingName;
		return bb.Event?.PlayerEvent?.MatchMakingName != null;
	}
}
