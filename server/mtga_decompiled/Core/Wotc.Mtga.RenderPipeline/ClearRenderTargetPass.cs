using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wotc.Mtga.RenderPipeline;

public class ClearRenderTargetPass : ScriptableRenderPass
{
	private ProfilingSampler _profilingSampler;

	public RTClearFlags RTClearFlags { private get; set; }

	public Color ClearColor { private get; set; }

	public float ClearDepth { private get; set; }

	public uint ClearStencil { private get; set; }

	public ClearRenderTargetPass(string name)
	{
		_profilingSampler = new ProfilingSampler(name);
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		CommandBuffer commandBuffer = CommandBufferPool.Get();
		using (new ProfilingScope(commandBuffer, _profilingSampler))
		{
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			commandBuffer.ClearRenderTarget(RTClearFlags, ClearColor, ClearDepth, ClearStencil);
		}
		context.ExecuteCommandBuffer(commandBuffer);
		commandBuffer.Clear();
		CommandBufferPool.Release(commandBuffer);
	}
}
