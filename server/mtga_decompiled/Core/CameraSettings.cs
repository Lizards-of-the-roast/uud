using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public class CameraSettings : MonoBehaviour
{
	[SerializeField]
	private OpaqueSortMode SortModeOpaque;

	[SerializeField]
	private TransparencySortMode SortModeTransparent;

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

	private void Start()
	{
		Cam.opaqueSortMode = SortModeOpaque;
		Cam.transparencySortMode = SortModeTransparent;
	}
}
