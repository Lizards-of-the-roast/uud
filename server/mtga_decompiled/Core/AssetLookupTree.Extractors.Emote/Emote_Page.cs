using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Emote;

public class Emote_Page : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.EmotePrefabData == null)
		{
			return false;
		}
		value = (int)bb.EmotePrefabData.Page;
		return true;
	}
}
