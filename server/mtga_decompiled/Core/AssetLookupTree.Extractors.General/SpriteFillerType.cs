using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class SpriteFillerType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.SpriteFillerType;
		return true;
	}
}
