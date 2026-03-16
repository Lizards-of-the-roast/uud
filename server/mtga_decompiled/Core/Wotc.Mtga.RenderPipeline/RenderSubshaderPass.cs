using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wotc.Mtga.RenderPipeline;

public class RenderSubshaderPass : ScriptableRenderPass
{
	private ProfilingSampler _profilingSampler;

	public List<ShaderTagId> ShaderTagIds { private get; set; }

	public SortingCriteria SortingCriteria { private get; set; }

	public RenderQueueRange RenderQueueRange { private get; set; }

	public LayerMask LayerMask { private get; set; }

	public RenderSubshaderPass(string name)
	{
		_profilingSampler = new ProfilingSampler(name);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		if (renderingData.cameraData.renderType != CameraRenderType.Overlay)
		{
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
			}
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}
	}
}
