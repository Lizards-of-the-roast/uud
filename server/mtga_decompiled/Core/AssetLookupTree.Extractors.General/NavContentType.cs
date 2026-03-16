using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class NavContentType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.NavContentType;
		return true;
	}
}
