using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Action;

public class Action_AlternativeGrpId : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.GreAction != null)
		{
			value = (int)bb.GreAction.AlternativeGrpId;
			return true;
		}
		value = 0;
		return false;
	}
}
