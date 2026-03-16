using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class UnitCount : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.UnitCount == 0)
		{
			return false;
		}
		value = bb.UnitCount;
		return true;
	}
}
