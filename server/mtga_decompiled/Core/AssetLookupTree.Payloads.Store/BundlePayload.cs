using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Store;

public class BundlePayload : StorePayload, IBackgroundColorPayload
{
	public AltAssetReference<Sprite> SpriteDataRef { get; set; } = new AltAssetReference<Sprite>();

	public Color BackgroundColor { get; set; }

	public override IEnumerable<string> GetFilePaths()
	{
		foreach (string filePath in base.GetFilePaths())
		{
			yield return filePath;
		}
		yield return SpriteDataRef.RelativePath;
	}
}
