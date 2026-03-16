using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.ManaMovement;

public class ManaMovement_SinkType : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.ManaMovement.IsValid)
		{
			value = (int)bb.ManaMovement.SinkType;
			return true;
		}
		value = 0;
		return false;
	}
}
