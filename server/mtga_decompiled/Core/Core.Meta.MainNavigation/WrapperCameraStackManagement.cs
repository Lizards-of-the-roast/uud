using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Wizards.GeneralUtilities;

namespace Core.Meta.MainNavigation;

public class WrapperCameraStackManagement : ScriptableObjectWithIdentifier
{
	private Camera _baseCamera;

	private UniversalAdditionalCameraData _baseCameraData;

	private readonly Dictionary<Camera, OverlayCameraInformation> _overlayCameras = new Dictionary<Camera, OverlayCameraInformation>();

	public void AssignBaseCamera(Camera camera)
	{
		if (_baseCamera != null)
		{
			Debug.LogErrorFormat("Trying to assign a base camera when one is already assigned!");
			return;
		}
		_baseCamera = camera;
		_baseCameraData = camera.GetComponent<UniversalAdditionalCameraData>();
	}

	public void AddOverlayCamera(OverlayCameraInformation overlayCameraInformation)
	{
		_overlayCameras[overlayCameraInformation.Camera] = overlayCameraInformation;
		List<Camera> first = new List<Camera>(_baseCameraData.cameraStack);
		_baseCameraData.cameraStack.Clear();
		_baseCameraData.cameraStack.AddRange((from x in first.Union(_overlayCameras.Values.Select((OverlayCameraInformation x) => x.Camera))
			orderby _overlayCameras.TryGetValue(x, out var value) ? value.Priority : 0
			select x).ToList());
	}
}
