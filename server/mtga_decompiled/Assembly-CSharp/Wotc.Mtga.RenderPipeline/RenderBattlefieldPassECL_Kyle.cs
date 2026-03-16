using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wotc.Mtga.RenderPipeline;

public class RenderBattlefieldPassECL_Kyle : ScriptableRenderPass
{
	private ProfilingSampler _profilingSampler;

	private RTHandle _rtBattlefieldB;

	private RTHandle _rtTempColor;

	public Material BlitMaterial { private get; set; }

	public List<ShaderTagId> ShaderTagIds { private get; set; }

	public SortingCriteria SortingCriteria { private get; set; }

	public RenderQueueRange RenderQueueRange { private get; set; }

	public LayerMask LayerMask { private get; set; }

	public RenderBattlefieldPassECL_Kyle(string name)
	{
		_profilingSampler = new ProfilingSampler(name);
	}

	public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
	{
		base.OnCameraSetup(cmd, ref renderingData);
		RenderTextureDescriptor descriptor = renderingData.cameraData.cameraTargetDescriptor;
		descriptor.depthBufferBits = 0;
		RenderingUtils.ReAllocateIfNeeded(ref _rtBattlefieldB, in descriptor);
		RenderingUtils.ReAllocateIfNeeded(ref _rtTempColor, in descriptor);
		ConfigureTarget(_rtBattlefieldB);
		ConfigureClear(ClearFlag.Color, Color.clear);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.cameraType == CameraType.Preview)
		{
			return;
		}
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, _profilingSampler))
		{
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			renderingData.cameraData.camera.TryGetCullingParameters(out var cullingParameters);
			CullingResults cullingResults = context.Cull(ref cullingParameters);
			DrawingSettings drawSettings = CreateDrawingSettings(ShaderTagIds, ref renderingData, SortingCriteria);
			FilteringSettings filteringSettings = new FilteringSettings(RenderQueueRange, LayerMask);
			RendererListParams param = new RendererListParams(cullingResults, drawSettings, filteringSettings);
			RendererList rendererList = context.CreateRendererList(ref param);
			commandBuffer.DrawRendererList(rendererList);
			commandBuffer.SetGlobalTexture("_BattlefieldBOpaques", _rtBattlefieldB);
			RTHandle cameraColorTargetHandle = renderingData.cameraData.renderer.cameraColorTargetHandle;
			if (cameraColorTargetHandle != null && _rtTempColor != null)
			{
				Blitter.BlitCameraTexture(commandBuffer, cameraColorTargetHandle, _rtTempColor, BlitMaterial, 0);
				Blitter.BlitCameraTexture(commandBuffer, _rtTempColor, cameraColorTargetHandle);
			}
		}
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
		CommandBufferPool.Release(commandBuffer);
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
