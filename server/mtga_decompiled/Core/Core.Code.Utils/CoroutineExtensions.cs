using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Code.Utils;

public static class CoroutineExtensions
{
	public static IEnumerator WaitOnAll(this IEnumerable<IEnumerator> coroutines, MonoBehaviour dispatcher)
	{
		int remaining = 0;
		foreach (IEnumerator coroutine in coroutines)
		{
			remaining++;
			dispatcher.StartCoroutine(Run(coroutine, delegate
			{
				remaining--;
			}));
		}
		while (remaining > 0)
		{
			yield return null;
		}
		static IEnumerator Run(IEnumerator context, Action done)
		{
			while (context.MoveNext())
			{
				yield return context.Current;
			}
			done();
		}
	}
}
