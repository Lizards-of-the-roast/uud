using System.Collections.Generic;
using AssetLookupTree.Payloads.Helpers;

namespace AssetLookupTree.Payloads.General;

public abstract class ClientOrGreLocKeyPayload : IPayload
{
	public readonly ClientOrGreLocKey LocKey = new ClientOrGreLocKey();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
