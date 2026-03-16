using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Browser;

public class ViewCounterUIPrefab : IPayload
{
	public readonly AltAssetReference<ViewCounter_UI> ViewCounterRef = new AltAssetReference<ViewCounter_UI>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return ViewCounterRef.RelativePath;
	}
}
