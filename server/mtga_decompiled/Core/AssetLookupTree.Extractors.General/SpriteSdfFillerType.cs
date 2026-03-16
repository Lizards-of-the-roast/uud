using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class SpriteSdfFillerType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.SpriteSdfFillerType;
		return true;
	}
}
