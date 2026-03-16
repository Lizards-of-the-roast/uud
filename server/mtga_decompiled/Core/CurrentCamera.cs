using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CurrentCamera : MonoBehaviour
{
	private static Camera _value;

	public static Action<Camera> OnCurrentCameraUpdated;

	private static readonly List<Camera> ActiveCameras = new List<Camera>();

	private Camera _camera;

	public static Camera Value
	{
		get
		{
			if (_value == null)
			{
				_value = Camera.main;
			}
			return _value;
		}
		private set
		{
			_value = value;
		}
	}

	private void Awake()
	{
		_camera = GetComponent<Camera>();
		if (_camera == null)
		{
			Debug.Log("Valid Camera component required on " + base.gameObject.name, base.gameObject);
			base.enabled = false;
		}
	}

	private void OnEnable()
	{
		ActiveCameras.Add(_camera);
		Value = _camera;
		OnCurrentCameraUpdated?.Invoke(Value);
	}

	private void OnDisable()
	{
		ActiveCameras.Remove(_camera);
		if (ActiveCameras.Count > 0)
		{
			Value = ActiveCameras[ActiveCameras.Count - 1];
			OnCurrentCameraUpdated?.Invoke(Value);
		}
	}
}
