using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.General;

public class PetId : IExtractor<string>
{
	public bool Execute(IBlackboard bb, out string value)
	{
		value = null;
		if (bb.PetId == null)
		{
			return false;
		}
		value = bb.PetId;
		return true;
	}
}
