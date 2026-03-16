using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Store;

public class PetPayload : IPayload, IBackgroundColorPayload
{
	public AltAssetReference<Sprite> SpriteDataRef { get; set; } = new AltAssetReference<Sprite>();

	public Color BackgroundColor { get; set; }

	public virtual IEnumerable<string> GetFilePaths()
	{
		yield return SpriteDataRef.RelativePath;
	}
}
