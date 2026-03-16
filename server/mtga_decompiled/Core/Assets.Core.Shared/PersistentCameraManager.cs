using UnityEngine;

namespace Assets.Core.Shared;

public sealed class PersistentCameraManager : MonoBehaviour
{
	private static PersistentCameraManager _instance;

	private Camera _persistentCamera;

	public static PersistentCameraManager Instance => _instance;

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			_persistentCamera = GetComponentInChildren<Camera>();
			SetPersistentCameraEnabledWhenNoCamera();
			Object.DontDestroyOnLoad(base.gameObject);
		}
	}

	public void SetPersistentCameraEnabled(bool enabled)
	{
		if (_persistentCamera != null)
		{
			_persistentCamera.gameObject.SetActive(enabled);
		}
	}

	private void SetPersistentCameraEnabledWhenNoCamera()
	{
		if (_persistentCamera != null)
		{
			_persistentCamera.gameObject.SetActive(Camera.current == null);
		}
	}
}
