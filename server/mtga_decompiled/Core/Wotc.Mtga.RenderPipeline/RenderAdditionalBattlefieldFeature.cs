using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wotc.Mtga.RenderPipeline;

public class RenderAdditionalBattlefieldFeature : ScriptableRendererFeature
{
	public class AdditionalBattlefieldPass : ScriptableRenderPass
	{
		public class SubpassConfig
		{
			public List<ShaderTagId> ShaderTagIds;

			public SortingCriteria SortingCriteria;

			public RenderQueueRange RenderQueueRange;

			public LayerMask LayerMask;
		}

		private ProfilingSampler _profilingSampler;

		private RTHandle _tempRT;

		public Material BlitMaterial { private get; set; }

		public string GlobalTexName { private get; set; }

		public RTHandle SharedBattlefieldRT { private get; set; }

		public SubpassConfig OpaqueSubpassConfig { get; set; }

		public SubpassConfig TransparentSubpassConfig { get; set; }

		public AdditionalBattlefieldPass(string name)
		{
			_profilingSampler = new ProfilingSampler(name);
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			base.OnCameraSetup(cmd, ref renderingData);
			if (BlitMaterial != null)
			{
				RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
				descriptor.autoGenerateMips = false;
				descriptor.useMipMap = false;
				descriptor.bindMS = false;
				descriptor.memoryless = RenderTextureMemoryless.Depth | RenderTextureMemoryless.MSAA;
				descriptor.msaaSamples = 1;
				descriptor.depthBufferBits = 0;
				RenderingUtils.ReAllocateIfNeeded(ref _tempRT, in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "TempRT-Blit");
			}
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			using (new ProfilingScope(commandBuffer, _profilingSampler))
			{
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
				DrawRendererList(commandBuffer, OpaqueSubpassConfig, context, ref renderingData);
				DrawRendererList(commandBuffer, TransparentSubpassConfig, context, ref renderingData);
				if (BlitMaterial != null)
				{
					commandBuffer.SetGlobalTexture(GlobalTexName, SharedBattlefieldRT);
					RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
					Blitter.BlitCameraTexture(commandBuffer, cameraColorTargetHandle, _tempRT, BlitMaterial, 0);
					Blitter.BlitCameraTexture(commandBuffer, _tempRT, cameraColorTargetHandle);
				}
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}

		private void DrawRendererList(CommandBuffer cmd, SubpassConfig config, ScriptableRenderContext context, ref RenderingData renderingData)
		{
			renderingData.cameraData.camera.TryGetCullingParameters(out var cullingParameters);
			CullingResults cullingResults = context.Cull(ref cullingParameters);
			DrawingSettings drawSettings = CreateDrawingSettings(config.ShaderTagIds, ref renderingData, config.SortingCriteria);
			FilteringSettings filteringSettings = new FilteringSettings(config.RenderQueueRange, config.LayerMask);
			RendererListParams param = new RendererListParams(cullingResults, drawSettings, filteringSettings);
			RendererList rendererList = context.CreateRendererList(ref param);
			cmd.DrawRendererList(rendererList);
		}

		public void Dispose()
		{
			_tempRT?.Release();
		}
	}

	[SerializeField]
	private string _globalTexName;

	[SerializeField]
	private LayerMask _layerMask;

	private AdditionalBattlefieldPass _pass;

	private RTHandle _sharedBattlefieldRT;

	private RTHandle _opaqueDepthRT;

	public Material BlitMatInstance { private get; set; }

	public override void Create()
	{
		_pass = new AdditionalBattlefieldPass(base.name);
		_pass.OpaqueSubpassConfig = new AdditionalBattlefieldPass.SubpassConfig
		{
			ShaderTagIds = new List<ShaderTagId>
			{
				new ShaderTagId("UniversalForward")
			}
		};
		_pass.TransparentSubpassConfig = new AdditionalBattlefieldPass.SubpassConfig
		{
			ShaderTagIds = new List<ShaderTagId>
			{
				new ShaderTagId("SRPDefaultUnlit"),
				new ShaderTagId("UniversalForward"),
				new ShaderTagId("UniversalForwardOnly")
			}
		};
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.cameraType != CameraType.Preview && renderingData.cameraData.renderType != CameraRenderType.Overlay)
		{
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.autoGenerateMips = false;
			descriptor.useMipMap = false;
			descriptor.bindMS = false;
			descriptor.memoryless = RenderTextureMemoryless.Depth | RenderTextureMemoryless.MSAA;
			descriptor.msaaSamples = 1;
			descriptor.depthBufferBits = 0;
			RenderingUtils.ReAllocateIfNeeded(ref _sharedBattlefieldRT, new Vector2(0.5f, 0.5f), in descriptor, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "SharedRT-color");
			RenderTextureDescriptor descriptor2 = renderingData.cameraData.cameraTargetDescriptor;
			descriptor2.autoGenerateMips = false;
			descriptor2.useMipMap = false;
			descriptor2.bindMS = false;
			descriptor2.memoryless = RenderTextureMemoryless.Depth | RenderTextureMemoryless.MSAA;
			descriptor2.msaaSamples = 1;
			descriptor2.depthBufferBits = 16;
			descriptor2.depthStencilFormat = GraphicsFormat.D16_UNorm;
			RenderingUtils.ReAllocateIfNeeded(ref _opaqueDepthRT, new Vector2(0.5f, 0.5f), in descriptor2, FilterMode.Point, TextureWrapMode.Repeat, isShadowMap: false, 1, 0f, "SharedRT-depth");
			_pass.SharedBattlefieldRT = _sharedBattlefieldRT;
			_pass.BlitMaterial = BlitMatInstance;
			_pass.GlobalTexName = _globalTexName;
			_pass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
			_pass.ConfigureInput(ScriptableRenderPassInput.None);
			_pass.ConfigureClear(ClearFlag.Color | ClearFlag.Depth, Color.black);
			_pass.ConfigureTarget(_sharedBattlefieldRT, _opaqueDepthRT);
			_pass.OpaqueSubpassConfig.RenderQueueRange = RenderQueueRange.opaque;
			_pass.OpaqueSubpassConfig.LayerMask = _layerMask;
			_pass.OpaqueSubpassConfig.SortingCriteria = SortingCriteria.CommonOpaque;
			_pass.TransparentSubpassConfig.RenderQueueRange = RenderQueueRange.transparent;
			_pass.TransparentSubpassConfig.LayerMask = _layerMask;
			_pass.TransparentSubpassConfig.SortingCriteria = SortingCriteria.CommonTransparent;
			renderer.EnqueuePass(_pass);
		}
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		_pass?.Dispose();
		_sharedBattlefieldRT?.Release();
		_opaqueDepthRT?.Release();
	}
}
