using GreClient.Rules;

namespace AssetLookupTree.Evaluators.Zone;

public abstract class Zone_CardCount_Base : ZoneEvaluatorBase_Int
{
	protected override int GetValue(MtgZone zone)
	{
		return zone.CardIds.Count;
	}
}
