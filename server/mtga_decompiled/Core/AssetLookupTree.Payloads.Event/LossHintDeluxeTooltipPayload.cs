using System.Collections.Generic;

namespace AssetLookupTree.Payloads.Event;

public class LossHintDeluxeTooltipPayload : IPayload
{
	public AltAssetReference<LossHintDeluxeTooltip> PrefabRef = new AltAssetReference<LossHintDeluxeTooltip>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return PrefabRef.RelativePath;
	}
}
