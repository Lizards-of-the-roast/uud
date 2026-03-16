using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class PrefabName : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.PrefabName;
		return !string.IsNullOrWhiteSpace(bb.PrefabName);
	}
}
