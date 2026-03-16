using System.Collections;
using UnityEngine;

namespace Supergenius;

public class TimeDisable : MonoBehaviour
{
	public float duration;

	public GameObject effect;

	private IEnumerator coroutine;

	private void OnEnable()
	{
		if (coroutine != null)
		{
			StopCoroutine(coroutine);
		}
		if (!effect.activeSelf)
		{
			StartEffect();
		}
		coroutine = TimeTicker();
		StartCoroutine(coroutine);
	}

	private IEnumerator TimeTicker()
	{
		yield return new WaitForSeconds(duration);
		EndEffect();
	}

	private void StartEffect()
	{
		effect.SetActive(value: true);
	}

	private void EndEffect()
	{
		effect.SetActive(value: false);
	}
}
