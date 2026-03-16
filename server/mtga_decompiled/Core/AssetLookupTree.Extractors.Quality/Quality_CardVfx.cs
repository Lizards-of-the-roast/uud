using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Quality;

public class Quality_CardVfx : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.CardVfxQuality;
		return true;
	}
}
