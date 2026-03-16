using UnityEngine;

public class CameraKeepWidth : MonoBehaviour
{
	[SerializeField]
	private float _frustumWidth = 100f;

	private uint _screenWidth;

	private uint _screenHeight;

	private Camera _camera;

	private Camera Cam
	{
		get
		{
			if (_camera == null)
			{
				_camera = GetComponent<Camera>();
			}
			return _camera;
		}
	}

	private void Update()
	{
		if (Screen.width != _screenWidth || Screen.height != _screenHeight)
		{
			_screenWidth = (uint)Screen.width;
			_screenHeight = (uint)Screen.height;
			if (Cam == null)
			{
				base.enabled = false;
				return;
			}
			float num = _frustumWidth / Cam.aspect;
			float magnitude = base.transform.localPosition.magnitude;
			Cam.fieldOfView = 2f * Mathf.Atan(num * 0.5f / magnitude) * 57.29578f;
		}
	}
}
