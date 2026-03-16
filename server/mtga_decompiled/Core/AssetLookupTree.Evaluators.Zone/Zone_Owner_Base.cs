using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Zone;

public abstract class Zone_Owner_Base : ZoneEvaluatorBase_List<GREPlayerNum>
{
	protected override GREPlayerNum GetValue(MtgZone zone)
	{
		return zone.OwnerNum;
	}
}
