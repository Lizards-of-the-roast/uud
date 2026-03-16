using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class Flavor : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.Flavor == null)
		{
			return false;
		}
		value = bb.Flavor;
		return true;
	}
}
