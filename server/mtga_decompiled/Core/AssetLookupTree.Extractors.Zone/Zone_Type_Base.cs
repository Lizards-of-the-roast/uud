using GreClient.Rules;

namespace AssetLookupTree.Extractors.Zone;

public abstract class Zone_Type_Base : ZoneExtractor
{
	protected override int ExtractValue(MtgZone zone)
	{
		return (int)zone.Type;
	}
}
