using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class PetLevel : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.PetLevel == 0)
		{
			return false;
		}
		value = bb.PetLevel;
		return true;
	}
}
