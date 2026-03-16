using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DualCapturePass : ScriptableRenderPass
{
	private Material material;

	private RenderTargetHandle firstTexture;

	private RenderTargetHandle secondTexture;

	private RenderTargetHandle tempTexture;

	public DualCapturePass(Material mat)
	{
		material = mat;
		firstTexture.Init("_FirstTexture");
		secondTexture.Init("_SecondTexture");
		tempTexture.Init("_TempTexture");
	}

	public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
	{
		RenderTextureDescriptor desc = cameraTextureDescriptor;
		desc.msaaSamples = 1;
		desc.depthBufferBits = 0;
		cmd.GetTemporaryRT(firstTexture.id, desc);
		cmd.GetTemporaryRT(secondTexture.id, desc);
		cmd.GetTemporaryRT(tempTexture.id, desc);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (!(material == null))
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get("DualCapturePass");
			RenderTargetIdentifier cameraColorTarget = renderingData.cameraData.renderer.cameraColorTarget;
			commandBuffer.Blit(cameraColorTarget, firstTexture.Identifier());
			commandBuffer.Blit(cameraColorTarget, secondTexture.Identifier());
			commandBuffer.SetGlobalTexture("_FirstTexture", firstTexture.Identifier());
			commandBuffer.SetGlobalTexture("_SecondTexture", secondTexture.Identifier());
			commandBuffer.Blit(cameraColorTarget, tempTexture.Identifier());
			commandBuffer.Blit(tempTexture.Identifier(), cameraColorTarget, material);
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
	}

	public override void FrameCleanup(CommandBuffer cmd)
	{
		cmd.ReleaseTemporaryRT(firstTexture.id);
		cmd.ReleaseTemporaryRT(secondTexture.id);
		cmd.ReleaseTemporaryRT(tempTexture.id);
	}
}
