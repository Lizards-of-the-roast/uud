using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Wrapper;

public class SettingsBackgroundPayload : IPayload
{
	public AltAssetReference<Sprite> MainPanelReference = new AltAssetReference<Sprite>();

	public AltAssetReference<Sprite> SubPanelReference = new AltAssetReference<Sprite>();

	public IEnumerable<string> GetFilePaths()
	{
		if (MainPanelReference != null)
		{
			yield return MainPanelReference.RelativePath;
		}
		if (SubPanelReference != null)
		{
			yield return SubPanelReference.RelativePath;
		}
	}
}
