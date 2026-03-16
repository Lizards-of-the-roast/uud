using UnityEngine;

namespace Wotc.Mtga.Unity.Utils;

public sealed class DestroyInBuilds : MonoBehaviour
{
	[Header("Disable the object instead of destroying it?")]
	public bool DisableInstead;

	private void Awake()
	{
		if (!Application.isEditor)
		{
			GameObject gameObject = base.gameObject;
			Debug.LogError("Object " + gameObject.name + " not cleaned up during build process, doing so now.");
			if (DisableInstead)
			{
				Object.Destroy(gameObject);
			}
			else
			{
				gameObject.SetActive(value: false);
			}
		}
	}
}
