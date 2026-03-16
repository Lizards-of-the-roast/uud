using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Cosmetics;

public class Cosmetic_StoreSKU : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.CosmeticStoreSKU;
		return !string.IsNullOrWhiteSpace(value);
	}
}
