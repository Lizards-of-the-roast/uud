using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Avatar;

public class Avatar_FramePartType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.AvatarFramePart;
		return true;
	}
}
