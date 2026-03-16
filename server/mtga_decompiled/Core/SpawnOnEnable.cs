using System;
using UnityEngine;

public class SpawnOnEnable : MonoBehaviour
{
	public GameObject prefab;

	[Tooltip("Use if instantiation is not frequent, but savings from unloading instance is.")]
	public bool destroyOnDissable = true;

	[Tooltip("Use this to delay instantiation incase of fiddly initilization timing issues. I.E. Sparky beacons.")]
	public float delay;

	public Action<GameObject> OnInstantiate;

	private GameObject _instance;

	private void OnEnable()
	{
		if (!_instance)
		{
			if (delay > 0f)
			{
				Invoke("MakeInstance", delay);
			}
			else
			{
				MakeInstance();
			}
		}
	}

	private void OnDisable()
	{
		if (destroyOnDissable && (bool)_instance)
		{
			UnityEngine.Object.Destroy(_instance);
		}
		CancelInvoke();
	}

	protected virtual void MakeInstance()
	{
		if ((bool)prefab)
		{
			_instance = UnityEngine.Object.Instantiate(prefab, base.transform);
			if ((bool)_instance)
			{
				OnInstantiate?.Invoke(_instance);
			}
		}
	}
}
