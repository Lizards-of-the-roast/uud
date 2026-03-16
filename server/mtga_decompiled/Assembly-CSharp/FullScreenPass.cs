using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FullScreenPass : ScriptableRenderPass
{
	private Material material;

	private RenderTargetHandle tempTexture;

	public FullScreenPass(Material mat)
	{
		material = mat;
		tempTexture.Init("_TempTexture");
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (!(material == null))
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get("FullScreenPostProcess");
			RenderTargetIdentifier cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
			RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
			cameraTargetDescriptor.msaaSamples = 1;
			commandBuffer.GetTemporaryRT(tempTexture.id, cameraTargetDescriptor);
			commandBuffer.Blit(cameraColorTarget, tempTexture.Identifier());
			commandBuffer.Blit(tempTexture.Identifier(), cameraColorTarget, material);
			commandBuffer.ReleaseTemporaryRT(tempTexture.id);
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
	}
}
