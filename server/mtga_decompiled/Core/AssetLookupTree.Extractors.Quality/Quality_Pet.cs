using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Quality;

public class Quality_Pet : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.PetQuality;
		return true;
	}
}
