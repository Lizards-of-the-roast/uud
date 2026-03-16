using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DistortionRenderFeature : ScriptableRendererFeature
{
	[Serializable]
	public class Settings
	{
		public string TextureName = "_GrabPassTransparent";

		public LayerMask LayerMask;

		public string KeywordName = "_DISTORTION";
	}

	private class GrabPass : ScriptableRenderPass
	{
		private RenderTargetHandle _tempColorTarget;

		private Settings _settings;

		private RenderTargetIdentifier _cameraTarget;

		public GrabPass(Settings settings)
		{
			_settings = settings;
			base.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
			_tempColorTarget.Init(settings?.TextureName ?? string.Empty);
		}

		public void Setup(RenderTargetIdentifier cameraTarget)
		{
			_cameraTarget = cameraTarget;
		}

		public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
		{
			cmd.GetTemporaryRT(_tempColorTarget.id, cameraTextureDescriptor);
			cmd.SetGlobalTexture(_settings.TextureName, _tempColorTarget.Identifier());
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			Blit(commandBuffer, _cameraTarget, _tempColorTarget.Identifier());
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}

		public override void FrameCleanup(CommandBuffer cmd)
		{
			cmd.ReleaseTemporaryRT(_tempColorTarget.id);
		}
	}

	private class RenderPass : ScriptableRenderPass
	{
		private FilteringSettings _filteringSettings;

		private RenderStateBlock _renderStateBlock = new RenderStateBlock(RenderStateMask.Nothing);

		private List<ShaderTagId> _shaderTagIdList = new List<ShaderTagId>
		{
			new ShaderTagId("SRPDefaultUnlit"),
			new ShaderTagId("UniversalForward"),
			new ShaderTagId("LightweightForward")
		};

		public RenderPass(Settings settings)
		{
			base.renderPassEvent = (RenderPassEvent)501;
			_filteringSettings = new FilteringSettings(RenderQueueRange.all, settings.LayerMask);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
			CommandBuffer commandBuffer = CommandBufferPool.Get();
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			DrawingSettings drawingSettings = CreateDrawingSettings(_shaderTagIdList, ref renderingData, SortingCriteria.CommonTransparent);
			context.DrawRenderers(renderingData.cullResults, ref drawingSettings, ref _filteringSettings, ref _renderStateBlock);
			context.ExecuteCommandBuffer(commandBuffer);
			commandBuffer.Clear();
			CommandBufferPool.Release(commandBuffer);
		}
	}

	private class KeywordPass : ScriptableRenderPass
	{
		private GlobalKeyword _globalKeyword;

		public KeywordPass(Settings settings)
		{
			base.renderPassEvent = RenderPassEvent.BeforeRenderingTransparents;
			_globalKeyword = GlobalKeyword.Create(settings?.KeywordName ?? string.Empty);
		}

		public override void OnCameraSetup(CommandBuffer cmd, ref RenderingData renderingData)
		{
			base.OnCameraSetup(cmd, ref renderingData);
			cmd.SetKeyword(in _globalKeyword, value: true);
		}

		public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
		{
		}

		public override void OnCameraCleanup(CommandBuffer cmd)
		{
			base.OnCameraCleanup(cmd);
			cmd.SetKeyword(in _globalKeyword, value: false);
		}
	}

	[SerializeField]
	private Settings _settings;

	private KeywordPass _keywordPass;

	private GrabPass _grabPass;

	private RenderPass _renderPass;

	public override void Create()
	{
		_keywordPass = new KeywordPass(_settings);
		_grabPass = new GrabPass(_settings);
		_renderPass = new RenderPass(_settings);
	}

	public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
	{
		base.SetupRenderPasses(renderer, in renderingData);
		_grabPass.Setup(renderer.cameraColorTarget);
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		renderer.EnqueuePass(_keywordPass);
		renderer.EnqueuePass(_grabPass);
		renderer.EnqueuePass(_renderPass);
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}
}
