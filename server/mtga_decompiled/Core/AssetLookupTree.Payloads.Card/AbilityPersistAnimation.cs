using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class AbilityPersistAnimation : IPayload
{
	public readonly AltAssetReference<AnimationClip> ClipRef = new AltAssetReference<AnimationClip>();

	public IEnumerable<string> GetFilePaths()
	{
		yield return ClipRef.RelativePath;
	}
}
