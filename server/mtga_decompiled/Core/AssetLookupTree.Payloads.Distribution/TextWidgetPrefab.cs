using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Distribution;

public class TextWidgetPrefab : IPayload
{
	public AltAssetReference<Widget_TextBox> TextWidgetRef = new AltAssetReference<Widget_TextBox>();

	public IEnumerable<string> GetFilePaths()
	{
		if (TextWidgetRef != null)
		{
			yield return TextWidgetRef.RelativePath;
		}
	}
}
