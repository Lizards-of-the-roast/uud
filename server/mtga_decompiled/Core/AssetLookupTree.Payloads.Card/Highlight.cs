using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class Highlight : IPayload
{
	public readonly AltAssetReference<GameObject> HighlightRef = new AltAssetReference<GameObject>();

	public readonly OffsetData OffsetData = new OffsetData();

	public IEnumerable<string> GetFilePaths()
	{
		yield return HighlightRef.RelativePath;
	}
}
