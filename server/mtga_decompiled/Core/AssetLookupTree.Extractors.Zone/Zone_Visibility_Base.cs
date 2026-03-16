using GreClient.Rules;

namespace AssetLookupTree.Extractors.Zone;

public abstract class Zone_Visibility_Base : ZoneExtractor
{
	protected override int ExtractValue(MtgZone zone)
	{
		return (int)zone.Visibility;
	}
}
