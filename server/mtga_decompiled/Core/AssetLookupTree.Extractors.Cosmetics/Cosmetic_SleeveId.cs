using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Cosmetics;

public class Cosmetic_SleeveId : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.CosmeticSleeveId;
		return !string.IsNullOrWhiteSpace(value);
	}
}
