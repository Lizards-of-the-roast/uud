using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Cosmetics;

public class Cosmetic_AccessoryMod : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.CosmeticAccessoryMod;
		return !string.IsNullOrWhiteSpace(value);
	}
}
