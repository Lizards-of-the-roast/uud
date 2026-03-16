using System.Collections.Generic;
using AssetLookupTree.Payloads.Helpers;

namespace AssetLookupTree.Payloads.Counter;

public class HangerEntry : IPayload
{
	public enum HeaderFormat
	{
		Default,
		Keyword_Counter
	}

	public HeaderFormat Format;

	public ClientOrGreLocKey Keyword = new ClientOrGreLocKey();

	public ClientOrGreLocKey Body = new ClientOrGreLocKey();

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
