using System;
using System.Collections.Generic;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class NumericInputWorkflow_ConfirmZeroPrompt : IPayload
{
	public bool ConfirmZero = true;

	public IEnumerable<string> GetFilePaths()
	{
		return Array.Empty<string>();
	}
}
