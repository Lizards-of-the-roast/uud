using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ImageBlitFeature : ScriptableRendererFeature
{
	[Serializable]
	public class Settings
	{
		public Texture2D testTexture;
	}

	private class ImageBlitPass : ScriptableRenderPass
	{
		private Texture2D texture;

		private Material blitMaterial;

		public ImageBlitPass(Texture2D tex)
		{
			texture = tex;
			Shader shader = Shader.Find("Hidden/BlitCopy");
			if (shader != null)
			{
				blitMaterial = new Material(shader);
			}
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (!(texture == null) && !(blitMaterial == null))
			{
				CommandBuffer commandBuffer = CommandBufferPool.Get("ImageBlitPass");
				_ = (RenderTargetIdentifier)renderingData.cameraData.renderer.cameraColorTargetHandle;
				context.ExecuteCommandBuffer(commandBuffer);
				CommandBufferPool.Release(commandBuffer);
			}
		}
	}

	public Settings settings = new Settings();

	private ImageBlitPass blitPass;

	public override void Create()
	{
		blitPass = new ImageBlitPass(settings.testTexture)
		{
			renderPassEvent = RenderPassEvent.AfterRendering
		};
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(blitPass);
	}
}
