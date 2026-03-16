using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class QualificationHanger : IPayload
{
	public readonly AltAssetReference<Sprite> IconRef = new AltAssetReference<Sprite>();

	public string LocTitleKey = string.Empty;

	public IEnumerable<string> GetFilePaths()
	{
		yield return IconRef.RelativePath;
	}
}
