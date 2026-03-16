using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wotc.Mtga.RenderPipeline;

public class Battlefield_ECL_subclass : ScriptableRendererFeature
{
	public class AdditionalBattlefieldPass : RenderSubshaderPass
	{
		private RTHandle _rtBattlefieldB;

		private RTHandle _rtTempColor;

		public Material BlitMaterial { private get; set; }

		public new List<ShaderTagId> ShaderTagIds { private get; set; }

		public new RenderQueueRange RenderQueueRange { private get; set; }

		public new SortingCriteria SortingCriteria { private get; set; }

		public new LayerMask LayerMask { private get; set; }

		public RenderSubshaderFeature BaseFeature { private get; set; }

		public AdditionalBattlefieldPass(string name)
			: base(name)
		{
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
			descriptor.depthBufferBits = 0;
			RenderingUtils.ReAllocateIfNeeded(ref _rtBattlefieldB, in descriptor);
			RenderingUtils.ReAllocateIfNeeded(ref _rtTempColor, in descriptor);
			ConfigureTarget(_rtBattlefieldB);
			ConfigureClear(ClearFlag.Color, Color.clear);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			if (renderingData.cameraData.cameraType != CameraType.Preview)
			{
				CommandBuffer commandBuffer = CommandBufferPool.Get();
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
				renderingData.cameraData.camera.TryGetCullingParameters(out var cullingParameters);
				CullingResults cullingResults = context.Cull(ref cullingParameters);
				DrawingSettings drawSettings = CreateDrawingSettings(ShaderTagIds, ref renderingData, SortingCriteria);
				FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange, LayerMask);
				RendererListParams param = new RendererListParams(cullingResults, drawSettings, filteringSettings);
				RendererList rendererList = context.CreateRendererList(ref param);
				commandBuffer.DrawRendererList(rendererList);
				commandBuffer.SetGlobalTexture("_BattlefieldB", _rtBattlefieldB);
				RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
				if (cameraColorTargetHandle != null && _rtTempColor != null)
				{
					Blitter.BlitCameraTexture(commandBuffer, cameraColorTargetHandle, _rtTempColor, BlitMaterial, 0);
					Blitter.BlitCameraTexture(commandBuffer, _rtTempColor, cameraColorTargetHandle);
				}
				context.ExecuteCommandBuffer(commandBuffer);
				commandBuffer.Clear();
				CommandBufferPool.Release(commandBuffer);
			}
		}

		public override void OnCameraCleanup(CommandBuffer cmd)
		{
			base.OnCameraCleanup(cmd);
		}

		public void Dispose()
		{
			_rtBattlefieldB?.Release();
			_rtTempColor?.Release();
		}
	}

	[SerializeField]
	private Material _blitMat;

	[SerializeField]
	private RenderQueueType _renderQueueType;

	[SerializeField]
	private LayerMask _layerMask;

	[SerializeField]
	private List<string> _shaderTags = new List<string>();

	[SerializeField]
	private RenderSubshaderFeature _baseFeature;

	private AdditionalBattlefieldPass _pass;

	private List<ShaderTagId> _shaderTagIds = new List<ShaderTagId>();

	public override void Create()
	{
		_pass = new AdditionalBattlefieldPass("Derived Pass");
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		_pass.renderPassEvent = RenderPassEvent.BeforeRenderingOpaques;
		_pass.ConfigureInput(ScriptableRenderPassInput.Normal);
		_pass.RenderQueueRange = ((_renderQueueType == RenderQueueType.Transparent) ? RenderQueueRange.transparent : RenderQueueRange.opaque);
		_pass.LayerMask = _layerMask;
		_pass.BlitMaterial = _blitMat;
		_pass.LayerMask = _layerMask;
		_pass.SortingCriteria = SortingCriteria.CommonTransparent;
		_shaderTagIds.Clear();
		for (int i = 0; i < _shaderTags.Count; i++)
		{
			_shaderTagIds.Add(new ShaderTagId(_shaderTags[i]));
		}
		_pass.ShaderTagIds = _shaderTagIds;
		renderer.EnqueuePass(_pass);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
		_pass?.Dispose();
	}
}
