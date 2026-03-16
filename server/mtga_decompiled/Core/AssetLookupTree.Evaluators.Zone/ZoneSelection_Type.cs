using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Zone;

public class ZoneSelection_Type : Zone_Type_Base
{
	protected override MtgZone GetZone(IBlackboard bb)
	{
		return bb.ZoneSelection;
	}
}
