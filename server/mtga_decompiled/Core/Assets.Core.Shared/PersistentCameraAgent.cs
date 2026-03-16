using UnityEngine;

namespace Assets.Core.Shared;

public class PersistentCameraAgent : MonoBehaviour
{
	[SerializeField]
	private GameObject _persistentCameraManagerPrefab;

	private void Awake()
	{
		if (_persistentCameraManagerPrefab == null)
		{
			Debug.LogWarning("Persistent camera agent has no persistent camera manager prefab refrence. Level transition rendering defects may happen.");
		}
		else if (PersistentCameraManager.Instance == null)
		{
			Object.Instantiate(_persistentCameraManagerPrefab);
		}
	}

	private void SetPersistentCameraEnabled(bool enabled)
	{
		if ((bool)PersistentCameraManager.Instance)
		{
			PersistentCameraManager.Instance.SetPersistentCameraEnabled(enabled);
		}
	}

	private void OnEnable()
	{
		SetPersistentCameraEnabled(enabled: false);
	}

	private void OnDisable()
	{
		SetPersistentCameraEnabled(enabled: true);
	}
}
