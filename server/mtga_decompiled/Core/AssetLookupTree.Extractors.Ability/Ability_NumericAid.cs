using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.Ability;

public class Ability_NumericAid : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = 0;
		if (bb.Ability == null)
		{
			return false;
		}
		value = (int)bb.Ability.NumericAid;
		return true;
	}
}
