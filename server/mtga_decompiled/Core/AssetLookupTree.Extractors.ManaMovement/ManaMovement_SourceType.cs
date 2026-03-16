using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.ManaMovement;

public class ManaMovement_SourceType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.ManaMovement.IsValid)
		{
			value = (int)bb.ManaMovement.SourceType;
			return true;
		}
		value = 0;
		return false;
	}
}
