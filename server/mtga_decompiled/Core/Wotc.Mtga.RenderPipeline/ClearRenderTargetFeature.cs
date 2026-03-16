using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wotc.Mtga.RenderPipeline;

public class ClearRenderTargetFeature : ScriptableRendererFeature
{
	[SerializeField]
	private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

	[SerializeField]
	private RTClearFlags _rtClearFlags;

	[SerializeField]
	private Color _clearColor = Color.black;

	[SerializeField]
	private float _clearDepth = 1f;

	[SerializeField]
	private uint _clearStencil;

	private ClearRenderTargetPass _pass;

	public override void Create()
	{
		_pass = new ClearRenderTargetPass(base.name);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		_pass.renderPassEvent = _renderPassEvent;
		_pass.RTClearFlags = _rtClearFlags;
		_pass.ClearColor = _clearColor;
		_pass.ClearDepth = _clearDepth;
		_pass.ClearStencil = _clearStencil;
		renderer.EnqueuePass(_pass);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
}
