using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Emote;

public class Emote_IncomingId : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.IncomingEmoteId;
		return !string.IsNullOrEmpty(value);
	}
}
