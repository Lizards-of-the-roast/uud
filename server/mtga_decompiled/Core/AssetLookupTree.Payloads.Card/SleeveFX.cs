using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class SleeveFX : IPayload
{
	public readonly AltAssetReference<GameObject> PrefabRef = new AltAssetReference<GameObject>();

	public readonly OffsetData OffsetData = new OffsetData();

	public readonly AudioEvent AudioEvent = new AudioEvent();

	public float CleanUpAfterSeconds;

	public bool AllowDuplicates = true;

	public IEnumerable<string> GetFilePaths()
	{
		yield return PrefabRef.RelativePath;
	}
}
