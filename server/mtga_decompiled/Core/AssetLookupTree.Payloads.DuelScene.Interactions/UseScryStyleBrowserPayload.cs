using System.Collections.Generic;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class UseScryStyleBrowserPayload : IPayload
{
	public bool InvertResponse;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
