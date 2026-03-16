using System.Collections.Generic;

namespace AssetLookupTree.Payloads.General;

public class Music : IPayload
{
	public bool IsState;

	public string MusicEvent = string.Empty;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
