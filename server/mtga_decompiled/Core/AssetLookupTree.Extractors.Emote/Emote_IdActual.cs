using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Emote;

public class Emote_IdActual : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.EmoteId;
		return !string.IsNullOrEmpty(value);
	}
}
