using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Extractors.Zone;

public class ToZone_Visibility : Zone_Visibility_Base
{
	protected override MtgZone GetZone(IBlackboard bb)
	{
		return bb.ToZone;
	}
}
