using System.Collections.Generic;

namespace AssetLookupTree.Payloads.UI.DuelScene;

public class ButtonNoticePrefab : IPayload
{
	public readonly AltAssetReference<NPEPrompt> ButtonPrompRef = new AltAssetReference<NPEPrompt>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return ButtonPrompRef.RelativePath;
	}
}
