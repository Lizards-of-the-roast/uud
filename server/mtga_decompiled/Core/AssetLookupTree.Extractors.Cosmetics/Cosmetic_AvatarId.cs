using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Cosmetics;

public class Cosmetic_AvatarId : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = bb.CosmeticAvatarId;
		return !string.IsNullOrWhiteSpace(value);
	}
}
