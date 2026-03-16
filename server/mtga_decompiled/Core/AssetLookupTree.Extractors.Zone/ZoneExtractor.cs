using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Extractors.Zone;

public abstract class ZoneExtractor : IExtractor<int>
{
	public bool Execute(IBlackboard bb, out int value)
	{
		MtgZone zone = GetZone(bb);
		value = 0;
		if (zone == null)
		{
			return false;
		}
		value = ExtractValue(zone);
		return true;
	}

	protected abstract int ExtractValue(MtgZone zone);

	protected abstract MtgZone GetZone(IBlackboard bb);
}
