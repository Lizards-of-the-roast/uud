using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Cosmetic;

public class PetPayload : IPayload
{
	public readonly AltAssetReference<GameObject> BattlefieldPrefab = new AltAssetReference<GameObject>();

	public readonly AltAssetReference<GameObject> WrapperPrefab = new AltAssetReference<GameObject>();

	public readonly AltAssetReference<Sprite> Icon = new AltAssetReference<Sprite>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return BattlefieldPrefab.RelativePath;
		yield return WrapperPrefab.RelativePath;
		yield return Icon.RelativePath;
	}
}
