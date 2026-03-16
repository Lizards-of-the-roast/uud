using System;
using System.Collections.Generic;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class ShouldShowConfirmationPayload : IPayload
{
	public bool ShowConfirmation = true;

	public IEnumerable<string> GetFilePaths()
	{
		return Array.Empty<string>();
	}
}
