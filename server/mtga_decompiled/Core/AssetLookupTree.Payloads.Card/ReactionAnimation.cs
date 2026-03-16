using System.Collections.Generic;
using UnityEngine;

namespace AssetLookupTree.Payloads.Card;

public class ReactionAnimation : IPayload
{
	public readonly AltAssetReference<AnimationClip> ClipRef = new AltAssetReference<AnimationClip>();

	public float Weight = 1f;

	public float Speed = 1f;

	public bool UseRandomSpeed;

	public float MinSpeed = 0.7f;

	public float MaxSpeed = 1.3f;

	public bool UseRandomStartTime;

	public float MinStartTime;

	public float MaxStartTime = 0.2f;

	public IEnumerable<string> GetFilePaths()
	{
		yield return ClipRef.RelativePath;
	}
}
