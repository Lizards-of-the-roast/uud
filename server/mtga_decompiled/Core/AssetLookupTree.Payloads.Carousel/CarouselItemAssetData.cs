using UnityEngine;

namespace AssetLookupTree.Payloads.Carousel;

public class CarouselItemAssetData
{
	public readonly AltAssetReference<GameObject> PrefabRef = new AltAssetReference<GameObject>();

	public readonly AltAssetReference<Sprite> MainSpriteRef = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<Sprite> FrameBreakSpriteRef = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<Sprite> FrameBreakBackgroundSpriteRef = new AltAssetReference<Sprite>();
}
