using System;
using System.Collections;
using System.Threading;
using UnityEngine;

namespace Core.Shared.Code;

public class GlobalCoroutineExecutor : MonoBehaviour
{
	public Coroutine StartGlobalCoroutine(IEnumerator routine, bool stopExisting = false)
	{
		if (stopExisting)
		{
			StopCoroutine(routine);
		}
		return StartCoroutine(routine);
	}

	public void StopGlobalCoroutine(Coroutine coroutine)
	{
		StopCoroutine(coroutine);
	}

	public void StartIntervalCoroutine(Action callback, float interval, CancellationToken cancellationToken)
	{
		StartCoroutine(RunInterval(callback, interval, cancellationToken));
	}

	private static IEnumerator RunInterval(Action callback, float interval, CancellationToken cancellationToken)
	{
		while (!cancellationToken.IsCancellationRequested)
		{
			yield return new WaitForSecondsRealtime(interval);
			callback();
		}
	}
}
