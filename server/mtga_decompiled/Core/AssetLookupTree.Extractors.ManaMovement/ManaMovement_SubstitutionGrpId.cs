using AssetLookupTree.Blackboard;

namespace AssetLookupTree.Extractors.ManaMovement;

public class ManaMovement_SubstitutionGrpId : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		if (bb.ManaMovement.IsValid)
		{
			value = (int)bb.ManaMovement.SubstitutionGrpId;
			return true;
		}
		value = 0;
		return false;
	}
}
