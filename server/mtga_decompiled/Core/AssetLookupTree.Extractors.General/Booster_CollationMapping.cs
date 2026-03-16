using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class Booster_CollationMapping : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.BoosterCollationMapping;
		return true;
	}
}
