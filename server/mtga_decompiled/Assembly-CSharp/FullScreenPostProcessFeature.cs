using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FullScreenPostProcessFeature : ScriptableRendererFeature
{
	public Material postProcessMaterial;

	private FullScreenPass pass;

	public override void Create()
	{
		pass = new FullScreenPass(postProcessMaterial);
		pass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (!(postProcessMaterial == null))
		{
			renderer.EnqueuePass(pass);
		}
	}
}
