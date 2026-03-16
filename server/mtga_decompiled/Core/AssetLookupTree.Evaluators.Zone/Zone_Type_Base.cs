using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Zone;

public abstract class Zone_Type_Base : ZoneEvaluatorBase_List<ZoneType>
{
	protected override ZoneType GetValue(MtgZone zone)
	{
		return zone.Type;
	}
}
