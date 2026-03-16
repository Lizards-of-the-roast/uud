using System.Collections;
using UnityEngine;

public class CameraShakeEvent : MonoBehaviour
{
	[Range(1f, 10f)]
	public int power = 1;

	public float delayForSeconds;

	private void Start()
	{
		StartCoroutine(ShakeAfterSeconds());
	}

	private IEnumerator ShakeAfterSeconds()
	{
		yield return new WaitForSeconds(delayForSeconds);
		BattlefieldManager battlefieldManager = Object.FindObjectOfType<BattlefieldManager>();
		if (!(battlefieldManager == null))
		{
			battlefieldManager.ShakeCamera(power);
		}
	}
}
