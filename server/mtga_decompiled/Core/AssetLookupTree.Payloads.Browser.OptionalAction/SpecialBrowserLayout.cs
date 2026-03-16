using System.Collections.Generic;
using AssetLookupTree.Payloads.Browser.Metadata;

namespace AssetLookupTree.Payloads.Browser.OptionalAction;

public class SpecialBrowserLayout : IPayload
{
	public OptionalActionSpecialBrowserLayoutData Data = new OptionalActionSpecialBrowserLayoutData();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
