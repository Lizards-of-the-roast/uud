using System.Collections.Generic;

namespace AssetLookupTree.Payloads.General;

public class Ambiance : IPayload
{
	public bool IsState;

	public string StartEvent = string.Empty;

	public string StopEvent = string.Empty;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
