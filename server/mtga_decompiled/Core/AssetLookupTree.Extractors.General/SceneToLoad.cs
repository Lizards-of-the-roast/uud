using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class SceneToLoad : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.SceneToLoad;
		return true;
	}
}
