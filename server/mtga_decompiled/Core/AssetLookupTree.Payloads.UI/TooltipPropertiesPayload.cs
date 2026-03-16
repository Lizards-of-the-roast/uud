using System.Collections.Generic;

namespace AssetLookupTree.Payloads.UI;

public class TooltipPropertiesPayload : IPayload
{
	public readonly AltAssetReference<TooltipPropertiesObject> PropertyPath = new AltAssetReference<TooltipPropertiesObject>();

	public IEnumerable<string> GetFilePaths()
	{
		if (PropertyPath != null)
		{
			yield return PropertyPath.RelativePath;
		}
	}
}
