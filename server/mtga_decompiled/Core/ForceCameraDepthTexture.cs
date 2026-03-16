using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class ForceCameraDepthTexture : MonoBehaviour
{
	public bool DisableDepth;

	private Camera _camera;

	private UniversalAdditionalCameraData _cameraData;

	private void Update()
	{
		if (_camera == null)
		{
			_camera = GetComponent<Camera>();
			if (_camera == null)
			{
				return;
			}
		}
		if (_cameraData == null)
		{
			_cameraData = GetComponent<UniversalAdditionalCameraData>();
		}
		DepthTextureMode depthTextureMode = _camera.depthTextureMode;
		CameraOverrideOption? cameraOverrideOption = _cameraData?.requiresDepthOption;
		bool? currentRequiresDepthTexture = _cameraData?.requiresDepthTexture;
		if (DisableDepth)
		{
			DepthTextureMode depthTextureMode2 = depthTextureMode & ~DepthTextureMode.Depth;
			CameraOverrideOption cameraOverrideOption2 = CameraOverrideOption.Off;
			bool flag = false;
			setDepthTextureMode(depthTextureMode, depthTextureMode2);
			setDepthOption(cameraOverrideOption ?? cameraOverrideOption2, cameraOverrideOption2);
			setRequiresDepth(currentRequiresDepthTexture ?? flag, flag);
		}
		else
		{
			DepthTextureMode depthTextureMode3 = depthTextureMode | DepthTextureMode.Depth;
			CameraOverrideOption cameraOverrideOption3 = CameraOverrideOption.On;
			bool flag2 = true;
			setDepthTextureMode(depthTextureMode, depthTextureMode3);
			setDepthOption(cameraOverrideOption ?? cameraOverrideOption3, cameraOverrideOption3);
			setRequiresDepth(currentRequiresDepthTexture ?? flag2, flag2);
		}
		void setDepthOption(CameraOverrideOption currentRequiresDepthOption, CameraOverrideOption requiresDepthOption)
		{
			if (requiresDepthOption != currentRequiresDepthOption)
			{
				_cameraData.requiresDepthOption = requiresDepthOption;
			}
		}
		void setDepthTextureMode(DepthTextureMode currentDepthTextureMode, DepthTextureMode depthTextureMode4)
		{
			if (depthTextureMode4 != currentDepthTextureMode)
			{
				_camera.depthTextureMode = depthTextureMode4;
			}
		}
		void setRequiresDepth(bool currentRequiesDepthTexture, bool requiresDepthTexture)
		{
			if (requiresDepthTexture != currentRequiresDepthTexture)
			{
				_cameraData.requiresDepthTexture = requiresDepthTexture;
			}
		}
	}
}
