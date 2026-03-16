using AssetLookupTree.Blackboard;
using Core.Meta.MainNavigation.SocialV2;

namespace AssetLookupTree.Extractors.General;

public class GatheringUserPrivilege : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		value = (int)bb.GatheringUserPrivilege;
		return bb.GatheringUserPrivilege != GatheringPrivilegeLevel.None;
	}
}
