using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Split_Test_Render_Feature : ScriptableRendererFeature
{
	[Serializable]
	public class Settings
	{
		public Color tint = Color.red;

		public Texture2D testTexture;
	}

	private class TintPass : ScriptableRenderPass
	{
		private Material mat;

		private Color tint;

		private RTHandle source;

		private RTHandle tempRT;

		public TintPass(Material material, Color tintColor)
		{
			mat = material;
			tint = tintColor;
			base.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			source = renderingData.cameraData.renderer.cameraColorTargetHandle;
			if (source == null)
			{
				Debug.LogError("TINT PASS SOURCE IS INVALID");
			}
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (mat == null || mat.shader == null)
			{
				Debug.LogError("MATERIALS OR SHADER IS FUCKED");
				return;
			}
			CommandBuffer commandBuffer = CommandBufferPool.Get("Screen Tint");
			mat.SetColor("_TintColor", Color.green);
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			RenderingUtils.ReAllocateIfNeeded(ref tempRT, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "_TempColorTex");
			_ = renderingData.cameraData.renderer.cameraColorTargetHandle;
			Blitter.BlitCameraTexture(commandBuffer, tempRT, source, mat, 0);
			context.ExecuteCommandBuffer(commandBuffer);
			CommandBufferPool.Release(commandBuffer);
		}
	}

	public Color tint = Color.red;

	public Settings settings = new Settings();

	private Material material;

	private TintPass tintPass;

	public override void Create()
	{
		Shader shader = Shader.Find("Hidden/ScreenTint");
		material = CoreUtils.CreateEngineMaterial(shader);
		Debug.Log(" HERE IS MY GENRATED MATERIALLLL #######  " + material);
		tintPass = new TintPass(material, settings.tint);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(tintPass);
	}

	protected override void Dispose(bool disposing)
	{
	}
}
