using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Zone;

public class Zone_Visibility : Zone_Visibility_Base
{
	protected override MtgZone GetZone(IBlackboard bb)
	{
		return bb.FromZone;
	}
}
