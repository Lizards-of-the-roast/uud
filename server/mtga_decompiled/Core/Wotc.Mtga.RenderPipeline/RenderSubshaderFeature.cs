using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Wotc.Mtga.RenderPipeline;

public class RenderSubshaderFeature : ScriptableRendererFeature
{
	[SerializeField]
	private RenderPassEvent _renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

	[SerializeField]
	private RenderQueueType _renderQueueType;

	[SerializeField]
	private LayerMask _layerMask;

	[SerializeField]
	private ScriptableRenderPassInput _requiredInputs;

	[SerializeField]
	private SortingCriteria _sortingCriteria = SortingCriteria.CommonOpaque;

	[SerializeField]
	private List<string> _shaderTags = new List<string>();

	private RenderSubshaderPass _pass;

	private List<ShaderTagId> _shaderTagIds = new List<ShaderTagId>();

	public override void Create()
	{
		_pass = new RenderSubshaderPass(base.name);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		_pass.renderPassEvent = _renderPassEvent;
		_pass.ConfigureInput(_requiredInputs);
		_pass.RenderQueueRange = ((_renderQueueType == RenderQueueType.Transparent) ? RenderQueueRange.transparent : RenderQueueRange.opaque);
		_pass.LayerMask = _layerMask;
		_pass.SortingCriteria = _sortingCriteria;
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
	}
}
