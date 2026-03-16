using GreClient.Rules;

namespace AssetLookupTree.Extractors.Zone;

public abstract class Zone_Owner_Base : ZoneExtractor
{
	protected override int ExtractValue(MtgZone zone)
	{
		return (int)zone.OwnerNum;
	}
}
