using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class PetVariantId : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.PetVariantId == null)
		{
			return false;
		}
		value = bb.PetVariantId;
		return true;
	}
}
