using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Carousel;

public class CarouselPayload : IPayload
{
	public readonly CarouselItemAssetData item = new CarouselItemAssetData();

	public bool IsPrefabBased => !string.IsNullOrEmpty(item?.PrefabRef?.RelativePath);

	public IEnumerable<string> GetFilePaths()
	{
		if (IsPrefabBased)
		{
			yield return item.PrefabRef.RelativePath;
		}
		yield return item.MainSpriteRef.RelativePath;
		yield return item.FrameBreakSpriteRef.RelativePath;
		yield return item.FrameBreakBackgroundSpriteRef.RelativePath;
	}
}
