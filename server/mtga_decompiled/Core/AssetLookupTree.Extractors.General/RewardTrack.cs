using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class RewardTrack : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.RewardTrack;
		return !string.IsNullOrWhiteSpace(bb.RewardTrack);
	}
}
