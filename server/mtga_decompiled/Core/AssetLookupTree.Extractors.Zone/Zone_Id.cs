using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Extractors.Zone;

public class Zone_Id : Zone_Id_Base
{
	protected override MtgZone GetZone(IBlackboard bb)
	{
		return bb.FromZone;
	}
}
