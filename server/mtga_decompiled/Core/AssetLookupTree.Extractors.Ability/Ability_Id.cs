using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Ability;

public class Ability_Id : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.Ability == null)
		{
			return false;
		}
		value = (int)bb.Ability.Id;
		return true;
	}
}
