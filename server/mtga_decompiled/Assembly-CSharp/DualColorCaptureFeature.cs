using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DualColorCaptureFeature : ScriptableRendererFeature
{
	[Header("Final Processing")]
	[SerializeField]
	private Material finalMaterial;

	public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingTransparents;

	private DualCapturePass pass;

	public override void Create()
	{
		pass = new DualCapturePass(finalMaterial);
		pass.renderPassEvent = renderPassEvent;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!(finalMaterial == null))
		{
			renderer.EnqueuePass(pass);
		}
	}
}
