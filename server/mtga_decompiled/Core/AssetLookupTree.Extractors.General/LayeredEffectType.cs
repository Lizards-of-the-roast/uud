using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class LayeredEffectType : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.LayeredEffectType;
		return !string.IsNullOrWhiteSpace(bb.LayeredEffectType);
	}
}
