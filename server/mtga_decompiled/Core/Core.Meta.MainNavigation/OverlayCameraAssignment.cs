using UnityEngine;

namespace Core.Meta.MainNavigation;

[RequireComponent(typeof(Camera))]
public class OverlayCameraAssignment : MonoBehaviour
{
	[SerializeField]
	private WrapperCameraStackManagement _cameraStackManager;

	[SerializeField]
	private int _priority;

	private void Start()
	{
		_cameraStackManager.AddOverlayCamera(new OverlayCameraInformation(GetComponent<Camera>(), _priority));
	}
}
