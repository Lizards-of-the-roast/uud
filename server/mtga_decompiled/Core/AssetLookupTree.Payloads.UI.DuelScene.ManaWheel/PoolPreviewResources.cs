using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Wotc.Mtga.DuelScene;
using Wotc.Mtga.DuelScene.UI;

namespace AssetLookupTree.Payloads.UI.DuelScene.ManaWheel;

public class PoolPreviewResources : IPayload
{
	public readonly AltAssetReference<CascadingManaPreviewWidget> PreviewWidget = new AltAssetReference<CascadingManaPreviewWidget>();

	public readonly AltAssetReference<GameObject> PreviewRowBacking = new AltAssetReference<GameObject>();

	public readonly AltAssetReference<Sprite> NullManaIcon = new AltAssetReference<Sprite>();

	public readonly AltAssetReference<ManaPoolSpriteTable> SpriteTableRef = new AltAssetReference<ManaPoolSpriteTable>();

	[JsonIgnore]
	public string WidgetPath => PreviewWidget?.RelativePath;

	[JsonIgnore]
	public string RowBackingPath => PreviewRowBacking?.RelativePath;

	[JsonIgnore]
	public string NullIconPath => NullManaIcon?.RelativePath;

	[JsonIgnore]
	public string SpriteTablePath => NullManaIcon?.RelativePath;

	public IEnumerable<string> GetFilePaths()
	{
		yield return PreviewWidget.RelativePath;
		yield return PreviewRowBacking.RelativePath;
		yield return NullManaIcon.RelativePath;
		yield return SpriteTableRef.RelativePath;
	}
}
