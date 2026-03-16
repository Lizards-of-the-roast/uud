using System.Collections.Generic;

namespace AssetLookupTree.Payloads.CardViewBuilder;

public class Prefabs : IPayload
{
	public readonly AltAssetReference<DuelScene_CDC> DuelSceneCdcPrefabRef = new AltAssetReference<DuelScene_CDC>();

	public readonly AltAssetReference<Meta_CDC> MetaCdcPrefabRef = new AltAssetReference<Meta_CDC>();

	public readonly AltAssetReference<CDCMetaCardView> MetaCardViewPrefabRef = new AltAssetReference<CDCMetaCardView>();

	public readonly AltAssetReference<ScaffoldingBase> DefaultScaffoldPrefabRef = new AltAssetReference<ScaffoldingBase>();

	public IEnumerable<string> GetFilePaths()
	{
		if (DuelSceneCdcPrefabRef != null)
		{
			yield return DuelSceneCdcPrefabRef.RelativePath;
		}
		if (MetaCdcPrefabRef != null)
		{
			yield return MetaCdcPrefabRef.RelativePath;
		}
		if (MetaCardViewPrefabRef != null)
		{
			yield return MetaCardViewPrefabRef.RelativePath;
		}
		if (DefaultScaffoldPrefabRef != null)
		{
			yield return DefaultScaffoldPrefabRef.RelativePath;
		}
	}
}
