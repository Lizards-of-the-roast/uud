using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Store;

public abstract class StorePayload : IPayload
{
	public readonly AltAssetReference<StoreItemDisplay> StoreDataRef = new AltAssetReference<StoreItemDisplay>();

	public readonly AltAssetReference<StoreItemDisplay> StoreConfirmDataRef = new AltAssetReference<StoreItemDisplay>();

	public virtual IEnumerable<string> GetFilePaths()
	{
		yield return StoreDataRef.RelativePath;
		yield return StoreConfirmDataRef.RelativePath;
	}
}
