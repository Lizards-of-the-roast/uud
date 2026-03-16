using System.Collections.Generic;

namespace AssetLookupTree.Payloads.DuelScene.Interactions;

public class ButtonStylePayload : IPayload
{
	public ButtonStyle.StyleType Style;

	public IEnumerable<string> GetFilePaths()
	{
		yield break;
	}
}
