using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.Zone;

public abstract class Zone_Visibility_Base : ZoneEvaluatorBase_List<Visibility>
{
	protected override Visibility GetValue(MtgZone zone)
	{
		return zone.Visibility;
	}
}
