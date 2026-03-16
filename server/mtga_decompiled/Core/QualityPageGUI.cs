using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Wizards.Mtga;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Quality;

public class QualityPageGUI : IDebugGUIPage
{
	private string[] _qualityModeCardVFXLabels;

	private string[] _qualityModePetLabels;

	private readonly GUIStyle _subsectionStyle = new GUIStyle
	{
		padding = new RectOffset(40, 0, 0, 0)
	};

	private DebugInfoIMGUIOnGui _GUI;

	public DebugInfoIMGUIOnGui.DebugTab TabType => DebugInfoIMGUIOnGui.DebugTab.Quality;

	public string TabName => "Quality";

	public bool HiddenInTab => false;

	public void Init(DebugInfoIMGUIOnGui gui)
	{
		_GUI = gui;
		_qualityModeCardVFXLabels = Enum.GetNames(typeof(QualityMode_CardVFX));
		_qualityModePetLabels = Enum.GetNames(typeof(QualityMode_Pet));
	}

	public void Destroy()
	{
	}

	public void OnQuit()
	{
	}

	public bool OnUpdate()
	{
		return true;
	}

	public void OnGUI()
	{
		UniversalRenderPipelineAsset universalRenderPipelineAsset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("Quality Menu", 100f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel($"Current Platform: {PlatformUtils.GetCurrentDeviceType()}", 400f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel($"Screen Resolution: {Screen.width}x{Screen.height} ({(float)Screen.width / (float)Screen.height})", 400f);
		GUILayout.EndHorizontal();
		string label = ((QualitySettings.vSyncCount == 0) ? $"FPS Limit: Vsync Inactive, Target FPS {Application.targetFrameRate}" : "FPS Limit: VSync Active");
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel(label, 400f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel($"Anti-Aliasing Level: {(MsaaQuality)universalRenderPipelineAsset.msaaSampleCount}", 400f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("Shadows", 400f);
		GUILayout.EndHorizontal();
		GUILayout.BeginVertical(_subsectionStyle);
		_GUI.ShowLabel("Shadows: " + (universalRenderPipelineAsset.supportsMainLightShadows ? "Active" : "Inactive"));
		_GUI.ShowLabel("Soft Shadows: " + (universalRenderPipelineAsset.supportsSoftShadows ? "Active" : "Inactive"));
		_GUI.ShowLabel($"Shadow Resolution: {universalRenderPipelineAsset.mainLightShadowmapResolution}");
		GUILayout.EndVertical();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("Bloom: " + (QualitySettingsUtil.Instance.Bloom ? "Active" : "Inactive"), 400f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("Motion Blur: " + (QualitySettingsUtil.Instance.MotionBlur ? "Active" : "Inactive"), 400f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("Ambient Occlusion: " + (QualitySettingsUtil.Instance.AmbientOcclusion ? "Active" : "Inactive"), 400f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("Distortion: " + (QualitySettingsUtil.Instance.Distortion ? "Active" : "Inactive"), 400f);
		GUILayout.EndHorizontal();
		GUILayout.BeginHorizontal();
		_GUI.ShowLabel("Outlines: " + (QualitySettingsUtil.Instance.Outlines ? "High" : "Low"), 400f);
		GUILayout.EndHorizontal();
		bool forceMinSpec = QualityModeProvider.ForceMinSpec;
		bool flag = _GUI.ShowToggle(forceMinSpec, "Force Minspec");
		if (forceMinSpec != flag)
		{
			QualityModeProvider.ForceMinSpec = flag;
		}
		if (!flag)
		{
			GUILayout.BeginHorizontal();
			_GUI.ShowLabel("Current Quality Level: " + QualitySettingsUtil.Instance.CurrentTierId, 400f);
			GUILayout.EndHorizontal();
			IReadOnlyList<string> availableTierNames = QualitySettingsUtil.Instance.AvailableTierNames;
			for (int i = 0; i < availableTierNames.Count; i++)
			{
				if (_GUI.ShowDebugButton(availableTierNames[i], 200f))
				{
					QualitySettingsUtil.Instance.GlobalQualityLevel = i;
				}
			}
			GUILayout.Space(20f);
			_GUI.ShowLabel("Quality ModeProvider", 200f);
			bool useDebugValues = QualityModeProvider.UseDebugValues;
			bool flag2 = _GUI.ShowToggle(useDebugValues, "Use Debug Values");
			if (useDebugValues != flag2)
			{
				QualityModeProvider.UseDebugValues = flag2;
			}
			if (flag2)
			{
				_GUI.ShowLabel("CardVFX:\t" + QualityModeProvider.DebugQualityModeCardVFX, 300f);
				QualityMode_CardVFX debugQualityModeCardVFX = QualityModeProvider.DebugQualityModeCardVFX;
				QualityMode_CardVFX qualityMode_CardVFX = (QualityMode_CardVFX)_GUI.ShowToolbar((int)debugQualityModeCardVFX, _qualityModeCardVFXLabels);
				if (debugQualityModeCardVFX != qualityMode_CardVFX)
				{
					QualityModeProvider.DebugQualityModeCardVFX = qualityMode_CardVFX;
				}
				_GUI.ShowLabel("Pets:\t" + QualityModeProvider.DebugQualityModePet, 300f);
				QualityMode_Pet debugQualityModePet = QualityModeProvider.DebugQualityModePet;
				QualityMode_Pet qualityMode_Pet = (QualityMode_Pet)_GUI.ShowToolbar((int)debugQualityModePet, _qualityModePetLabels);
				if (debugQualityModePet != qualityMode_Pet)
				{
					QualityModeProvider.DebugQualityModePet = qualityMode_Pet;
				}
			}
		}
		else
		{
			QualityModeProvider.UseDebugValues = false;
			if (flag != forceMinSpec)
			{
				QualitySettingsUtil.Instance.GlobalQualityLevel = 0;
			}
		}
		if (_GUI.ShowDebugButton($"Disable Asset Pools {QualityModeProvider.DebugDisableAssetPool}", 500f))
		{
			QualityModeProvider.DebugDisableAssetPool = !QualityModeProvider.DebugDisableAssetPool;
		}
		if (_GUI.ShowDebugButton($"Disable Object Pools {QualityModeProvider.DebugDisableObjectPool}", 500f))
		{
			QualityModeProvider.DebugDisableObjectPool = !QualityModeProvider.DebugDisableObjectPool;
		}
	}
}
