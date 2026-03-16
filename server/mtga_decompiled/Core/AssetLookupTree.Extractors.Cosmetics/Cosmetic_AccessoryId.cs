using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Cosmetics;

public class Cosmetic_AccessoryId : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.CosmeticAccessoryId;
		return !string.IsNullOrWhiteSpace(value);
	}
}
