using System;
using System.Collections;
using UnityEngine;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.VFX;

public class DelayVFXCallback : MonoBehaviour
{
	public void StartDelay(float delay, GameObject vfx, Action<DelayVFXCallback> onComplete)
	{
		if (!vfx || delay <= 0f)
		{
			onComplete?.Invoke(this);
			return;
		}
		vfx.UpdateActive(active: false);
		StartCoroutine(DelayTimer(delay, vfx, onComplete));
	}

	private IEnumerator DelayTimer(float delay, GameObject vfx, Action<DelayVFXCallback> onComplete)
	{
		yield return new WaitForSeconds(delay);
		vfx.UpdateActive(active: true);
		onComplete?.Invoke(this);
	}
}
