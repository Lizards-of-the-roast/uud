using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Emote;

public class Emote_Id : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.EmotePrefabData == null)
		{
			return false;
		}
		value = bb.EmotePrefabData.Id;
		return true;
	}
}
