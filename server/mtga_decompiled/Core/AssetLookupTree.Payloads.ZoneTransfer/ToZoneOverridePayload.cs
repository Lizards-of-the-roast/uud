using System.Collections.Generic;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Payloads.ZoneTransfer;

public class ToZoneOverridePayload : IPayload
{
	public ZoneType ZoneType;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
