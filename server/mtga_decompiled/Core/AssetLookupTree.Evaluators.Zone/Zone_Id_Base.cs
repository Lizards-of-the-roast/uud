using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Zone;

public abstract class Zone_Id_Base : ZoneEvaluatorBase_Int
{
	protected override int GetValue(MtgZone zone)
	{
		return (int)zone.Id;
	}
}
