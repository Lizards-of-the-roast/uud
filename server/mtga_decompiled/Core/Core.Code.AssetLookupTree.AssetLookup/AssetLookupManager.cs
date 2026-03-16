using AssetLookupTree;

namespace Core.Code.AssetLookupTree.AssetLookup;

public class AssetLookupManager
{
	public AssetLookupSystem AssetLookupSystem;

	public AssetLookupManager(AssetLookupSystem lookupSystem)
	{
		AssetLookupSystem = lookupSystem;
	}
}
