using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.RenderPipeline;

namespace Wotc.Mtga.Quality;

public class QualitySettingsUtil : ScriptableObject
{
	[Serializable]
	public struct GraphicsTier
	{
		public string Name;

		public UniversalRenderPipelineAsset Asset;

		public ScriptableRendererFeature SSAO;

		public ScriptableRendererFeature Distortion;

		public ScriptableRendererFeature SobelOutlines;

		public ScriptableRendererFeature HybridBattlefield;

		public int VSync;

		public int Bloom;

		public int MotionBlur;
	}

	public const string GLOBAL_QUALITY_LEVEL_SETTING_NAME = "Graphics.GlobalQualityLevel";

	public const string CUSTOM_SETTING_SUFFIX = ".Custom";

	private static QualitySettingsUtil _instance;

	[SerializeField]
	private VolumeProfile _postProcessingProfile;

	[SerializeField]
	private GraphicsTier[] _allTiers;

	[SerializeField]
	private UniversalRenderPipelineAsset _customAsset;

	private GraphicsTier _currentTier;

	private IReadOnlyList<GraphicsTier> _availableTiers;

	private IReadOnlyList<string> _availableTierNames;

	private IReadOnlyList<QualitySettingModifier> _qualityModifiers;

	private bool _hybridBattlefieldEnabled;

	public static QualitySettingsUtil Instance => _instance ?? (_instance = Resources.Load<QualitySettingsUtil>("QualitySettingsUtil"));

	public bool Bloom
	{
		get
		{
			if (_postProcessingProfile.TryGet<Bloom>(out var component))
			{
				return component.active;
			}
			return false;
		}
	}

	public bool MotionBlur
	{
		get
		{
			if (_postProcessingProfile.TryGet<MotionBlur>(out var component))
			{
				return component.active;
			}
			return false;
		}
	}

	public bool AmbientOcclusion
	{
		get
		{
			if ((bool)_currentTier.SSAO)
			{
				return _currentTier.SSAO.isActive;
			}
			return false;
		}
	}

	public bool Distortion
	{
		get
		{
			if ((bool)_currentTier.Distortion)
			{
				return _currentTier.Distortion.isActive;
			}
			return false;
		}
	}

	public bool Outlines
	{
		get
		{
			if ((bool)_currentTier.SobelOutlines)
			{
				return _currentTier.SobelOutlines.isActive;
			}
			return false;
		}
	}

	public bool IsCustomTier => _currentTier.Asset == _customAsset;

	public string CurrentTierId => _currentTier.Name;

	public IReadOnlyList<GraphicsTier> AvailableTiers => LazyInitializer.EnsureInitialized(ref _availableTiers, LazyLoadGraphicsTiers);

	public IReadOnlyList<string> AvailableTierNames => LazyInitializer.EnsureInitialized(ref _availableTierNames, LazyLoadGraphicsTierNames);

	public IReadOnlyList<QualitySettingModifier> QualityModifiers => LazyInitializer.EnsureInitialized(ref _qualityModifiers, LazyLoadQualityModifiers);

	public int GlobalQualityLevel
	{
		get
		{
			return Mathf.Clamp(PlayerPrefsExt.GetInt("Graphics.GlobalQualityLevel"), 0, AvailableTiers.Count - 1);
		}
		set
		{
			value = Mathf.Clamp(value, 0, AvailableTiers.Count - 1);
			PlayerPrefsExt.SetInt("Graphics.GlobalQualityLevel", value);
			PlayerPrefsExt.Save();
			ApplySettings();
		}
	}

	public void ApplySettings()
	{
		int qualityLevel = QualitySettings.GetQualityLevel();
		int globalQualityLevel = GlobalQualityLevel;
		_currentTier = AvailableTiers[globalQualityLevel];
		QualitySettings.SetQualityLevel(globalQualityLevel);
		QualitySettings.renderPipeline = QualitySettings.GetRenderPipelineAssetAt(globalQualityLevel);
		if (globalQualityLevel != qualityLevel)
		{
			LoadCustomSettings();
		}
		foreach (QualitySettingModifier qualityModifier in QualityModifiers)
		{
			qualityModifier.UpdateSetting();
		}
		PlatformContext.GetQualitySelector().ApplyPlatformQualitySettings();
	}

	public void LoadCustomSettings()
	{
		if (!IsCustomTier)
		{
			return;
		}
		foreach (QualitySettingModifier qualityModifier in QualityModifiers)
		{
			string key = qualityModifier.SettingName + ".Custom";
			if (PlayerPrefs.HasKey(key))
			{
				qualityModifier.Set(PlayerPrefs.GetInt(key));
			}
		}
	}

	public void SaveCustomSettings()
	{
		if (!IsCustomTier)
		{
			return;
		}
		foreach (QualitySettingModifier qualityModifier in QualityModifiers)
		{
			PlayerPrefs.SetInt(qualityModifier.SettingName + ".Custom", qualityModifier.CurrentValue);
		}
		PlayerPrefs.Save();
	}

	public void SetHybridBattlefieldEnabled(bool enabled, bool? customEnabled = null)
	{
		_hybridBattlefieldEnabled = enabled;
		foreach (GraphicsTier availableTier in _availableTiers)
		{
			if ((bool)availableTier.HybridBattlefield)
			{
				availableTier.HybridBattlefield.SetActive(enabled && (availableTier.Asset != _customAsset || customEnabled == true));
			}
		}
	}

	public void SetHybridBattlefieldMatInstance(Material matInstance)
	{
		foreach (GraphicsTier availableTier in _availableTiers)
		{
			if (availableTier.HybridBattlefield is RenderAdditionalBattlefieldFeature renderAdditionalBattlefieldFeature)
			{
				renderAdditionalBattlefieldFeature.BlitMatInstance = matInstance;
			}
		}
	}

	private IReadOnlyList<GraphicsTier> LazyLoadGraphicsTiers()
	{
		string[] names = QualitySettings.names;
		List<GraphicsTier> list = new List<GraphicsTier>(_allTiers.Length);
		GraphicsTier[] allTiers = _allTiers;
		for (int i = 0; i < allTiers.Length; i++)
		{
			GraphicsTier item = allTiers[i];
			for (int j = 0; j < names.Length; j++)
			{
				if (item.Asset == QualitySettings.GetRenderPipelineAssetAt(j))
				{
					list.Add(item);
					break;
				}
			}
		}
		return list;
	}

	private IReadOnlyList<string> LazyLoadGraphicsTierNames()
	{
		IReadOnlyList<GraphicsTier> availableTiers = AvailableTiers;
		string[] array = new string[availableTiers.Count];
		for (int i = 0; i < availableTiers.Count; i++)
		{
			array[i] = availableTiers[i].Name;
		}
		return array;
	}

	private static void ApplyShadowQualityLevel(int q)
	{
		UniversalRenderPipelineAsset asset = (UniversalRenderPipelineAsset)QualitySettings.renderPipeline;
		switch (q)
		{
		case 0:
			asset.SetMainLightShadowsEnabled(value: false);
			asset.SetAdditionalLightShadowsEnabled(value: false);
			asset.SetSoftShadowsSupported(value: false);
			asset.SetShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._256);
			break;
		default:
			asset.SetMainLightShadowsEnabled(value: true);
			asset.SetAdditionalLightShadowsEnabled(value: true);
			asset.SetSoftShadowsSupported(value: false);
			asset.SetShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._512);
			break;
		case 2:
			asset.SetMainLightShadowsEnabled(value: true);
			asset.SetAdditionalLightShadowsEnabled(value: true);
			asset.SetSoftShadowsSupported(value: true);
			asset.SetShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._1024);
			break;
		case 3:
			asset.SetMainLightShadowsEnabled(value: true);
			asset.SetAdditionalLightShadowsEnabled(value: true);
			asset.SetSoftShadowsSupported(value: true);
			asset.SetShadowResolution(UnityEngine.Rendering.Universal.ShadowResolution._2048);
			break;
		}
	}

	private IReadOnlyList<QualitySettingModifier> LazyLoadQualityModifiers()
	{
		return new QualitySettingModifier[9]
		{
			new QualitySettingModifier("Graphics.VSync", "FPS Limit", 3, (int v) => QualitySettingsHelpers.FPSCapLevelNames[v], delegate(int v)
			{
				if (!IsCustomTier)
				{
					v = _currentTier.VSync;
				}
				Application.targetFrameRate = QualitySettingsHelpers.FPSLevels[v];
				QualitySettings.vSyncCount = ((v == 0) ? 1 : 0);
			}),
			new QualitySettingModifier("Graphics.AAQualityLevel", "Anti-Aliasing", 3, (int v) => v switch
			{
				2 => "Medium", 
				1 => "Low", 
				0 => "Off", 
				_ => "High", 
			}, delegate(int v)
			{
				if (IsCustomTier)
				{
					((UniversalRenderPipelineAsset)QualitySettings.renderPipeline).msaaSampleCount = v switch
					{
						2 => 4, 
						1 => 2, 
						0 => 1, 
						_ => 8, 
					};
				}
			}),
			new QualitySettingModifier("Graphics.ShadowQualityLevel", "Shadows", 3, (int v) => QualitySettingsHelpers.ShadowLevelQualityNames[v], delegate(int v)
			{
				if (IsCustomTier)
				{
					ApplyShadowQualityLevel(v);
				}
			}),
			new QualitySettingModifier("Graphics.Bloom", "Bloom", 1, (int v) => (v <= 0) ? "Off" : "On", delegate(int v)
			{
				if (!IsCustomTier)
				{
					v = _currentTier.Bloom;
				}
				if (_postProcessingProfile.TryGet<Bloom>(out var component))
				{
					component.active = v == 1;
				}
			}),
			new QualitySettingModifier("Graphics.MotionBlur", "Motion Blur", 1, (int v) => (v <= 0) ? "Off" : "On", delegate(int v)
			{
				if (!IsCustomTier)
				{
					v = _currentTier.MotionBlur;
				}
				if (_postProcessingProfile.TryGet<MotionBlur>(out var component))
				{
					component.active = v == 1;
				}
			}),
			new QualitySettingModifier("Graphics.AmbientOcclusion", "Ambient Occlusion", 1, (int v) => (v <= 0) ? "Off" : "On", delegate(int v)
			{
				if (IsCustomTier)
				{
					_currentTier.SSAO.SetActive(v == 1);
				}
			}),
			new QualitySettingModifier("Graphics.Distortion", "Distortion", 1, (int v) => (v <= 0) ? "Off" : "On", delegate(int v)
			{
				if (IsCustomTier)
				{
					_currentTier.Distortion.SetActive(v > 0);
				}
			}),
			new QualitySettingModifier("Graphics.OutlineQuality", "Outlines", 1, (int v) => (v <= 0) ? "Low" : "High", delegate(int v)
			{
				if (IsCustomTier)
				{
					_currentTier.SobelOutlines.SetActive(v > 0);
				}
			}),
			new QualitySettingModifier("Graphics.HybridBattlefield", "BattlefieldMultiRender", 1, (int v) => (v <= 0) ? "Off" : "On", delegate(int v)
			{
				if (IsCustomTier)
				{
					SetHybridBattlefieldEnabled(_hybridBattlefieldEnabled, v > 0);
				}
			})
		};
	}
}
