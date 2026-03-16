using System.Collections;
using UnityEngine;

public class DelayVFX : MonoBehaviour
{
	public GameObject vfxChildToDelay;

	public float Delay;

	private bool skipNextDelay;

	private void OnEnable()
	{
		StartCoroutine(StartVFX());
	}

	private void OnDisable()
	{
		vfxChildToDelay.SetActive(value: false);
	}

	private IEnumerator StartVFX()
	{
		if (!skipNextDelay)
		{
			yield return new WaitForSeconds(Delay);
		}
		else
		{
			skipNextDelay = false;
		}
		vfxChildToDelay.SetActive(value: true);
	}

	public void SkipNextDelay()
	{
		skipNextDelay = true;
	}
}
