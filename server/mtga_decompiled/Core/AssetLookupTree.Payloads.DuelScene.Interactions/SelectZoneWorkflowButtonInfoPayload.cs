using System.Collections.Generic;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class SelectZoneWorkflowButtonInfoPayload : IPayload
{
	public bool IsTopSelection;

	public bool IsMainButton;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
