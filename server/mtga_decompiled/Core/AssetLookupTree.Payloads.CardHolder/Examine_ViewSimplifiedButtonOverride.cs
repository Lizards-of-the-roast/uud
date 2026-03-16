using System;
using System.Collections.Generic;

namespace AssetLookupTree.Payloads.CardHolder;

public class Examine_ViewSimplifiedButtonOverride : IPayload
{
	public bool ShowViewSimplifiedToggle;

	public IEnumerable<string> GetFilePaths()
	{
		return Array.Empty<string>();
	}
}
