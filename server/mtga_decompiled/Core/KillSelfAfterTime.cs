using System;
using UnityEngine;
using Wotc.Mtga.Extensions;

[Obsolete("Use SelfCleanup instead.")]
public class KillSelfAfterTime : MonoBehaviour
{
	public float lifetime;

	private void Awake()
	{
		ReplaceWithSelfCleanup();
	}

	[ContextMenu("Replace")]
	public void ReplaceWithSelfCleanup()
	{
		base.gameObject.AddOrGetComponent<SelfCleanup>().SetLifetime(lifetime, SelfCleanup.CleanupType.Destroy);
		UnityEngine.Object.DestroyImmediate(this);
	}
}
