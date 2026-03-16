using UnityEngine;

namespace Core.Meta.MainNavigation;

[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class BaseCamera : MonoBehaviour
{
	[SerializeField]
	private WrapperCameraStackManagement _cameraStackManager;

	private void Awake()
	{
		_cameraStackManager.AssignBaseCamera(GetComponent<Camera>());
	}
}
