using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Wotc.Mtga.Extensions;

public static class URPExtensions
{
	private static void MethodExtension<T>(this UniversalRenderPipelineAsset pipelineAsset, string fieldName, T value)
	{
		FieldInfo field = typeof(UniversalRenderPipelineAsset).GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
		if (field == null)
		{
			Debug.LogError("Could not find field with name " + fieldName + " in UniversalRenderPipelineAsset");
		}
		else
		{
			field.SetValue(pipelineAsset, value);
		}
	}

	public static void SetMainLightShadowsEnabled(this UniversalRenderPipelineAsset asset, bool value)
	{
		asset.MethodExtension("m_MainLightShadowsSupported", value);
	}

	public static void SetAdditionalLightShadowsEnabled(this UniversalRenderPipelineAsset asset, bool value)
	{
		asset.MethodExtension("m_AdditionalLightShadowsSupported", value);
	}

	public static void SetSoftShadowsSupported(this UniversalRenderPipelineAsset asset, bool value)
	{
		asset.MethodExtension("m_SoftShadowsSupported", value);
	}

	public static void SetShadowResolution(this UniversalRenderPipelineAsset asset, UnityEngine.Rendering.Universal.ShadowResolution value)
	{
		asset.MethodExtension("m_MainLightShadowmapResolution", (int)value);
		asset.MethodExtension("m_AdditionalLightsShadowmapResolution", (int)value);
	}
}
