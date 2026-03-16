using UnityEngine;
using UnityEngine.Rendering.Universal;

[ExecuteInEditMode]
public class DepthTexture : MonoBehaviour
{
	private void Start()
	{
		GetComponent<Camera>().depthTextureMode |= DepthTextureMode.Depth;
		UniversalAdditionalCameraData universalAdditionalCameraData = GetComponent<UniversalAdditionalCameraData>();
		if (universalAdditionalCameraData == null)
		{
			universalAdditionalCameraData = base.gameObject.AddComponent<UniversalAdditionalCameraData>();
		}
		universalAdditionalCameraData.requiresDepthOption = CameraOverrideOption.On;
		universalAdditionalCameraData.requiresDepthTexture = true;
	}
}
