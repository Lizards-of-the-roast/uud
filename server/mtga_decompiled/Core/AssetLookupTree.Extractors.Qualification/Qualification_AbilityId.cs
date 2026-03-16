using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Qualification;

public class Qualification_AbilityId : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.Qualification.HasValue)
		{
			value = (int)bb.Qualification.Value.AbilityId;
			return true;
		}
		value = 0;
		return false;
	}
}
