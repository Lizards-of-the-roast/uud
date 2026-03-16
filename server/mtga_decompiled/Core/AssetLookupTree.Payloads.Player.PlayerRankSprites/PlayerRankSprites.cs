using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Player.PlayerRankSprites;

public class PlayerRankSprites : IPayload
{
	public AltAssetReference<Sprite> SpriteRef = new AltAssetReference<Sprite>();

	public AltAssetReference<Sprite> GemOverlayRef = new AltAssetReference<Sprite>();

	public AltAssetReference<Sprite> RankBaseRef = new AltAssetReference<Sprite>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return SpriteRef.RelativePath;
		if (!string.IsNullOrEmpty(GemOverlayRef.RelativePath))
		{
			yield return GemOverlayRef.RelativePath;
		}
		if (!string.IsNullOrEmpty(RankBaseRef.RelativePath))
		{
			yield return RankBaseRef.RelativePath;
		}
	}
}
