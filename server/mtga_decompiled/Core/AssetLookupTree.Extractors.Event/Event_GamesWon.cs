using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Event;

public class Event_GamesWon : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (bb.Event?.PostMatchContext?.GamesWon).GetValueOrDefault();
		return bb.Event?.PostMatchContext != null;
	}
}
