using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Action;

public class Action_AbilityGrpId : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.GreAction != null)
		{
			value = (int)bb.GreAction.AbilityGrpId;
			return true;
		}
		value = 0;
		return false;
	}
}
