using System;
using UnityEngine;

public static class MaterialExtensions
{
	public static bool HasNonNullTexture(this Material mat, string property)
	{
		if (mat.HasProperty(property))
		{
			return mat.GetTexture(property) != null;
		}
		return false;
	}

	public static bool TryGetToggle(this Material mat, string property, bool defaultVal)
	{
		if (mat.HasProperty(property))
		{
			return Math.Abs(mat.GetFloat(property) - 1f) < float.Epsilon;
		}
		return defaultVal;
	}

	public static void SetToggle(this Material mat, string property, bool state)
	{
		mat.SetFloat(property, state ? 1f : 0f);
	}

	public static void SetKeyword(this Material mat, string keyword, bool state)
	{
		if (state)
		{
			mat.EnableKeyword(keyword);
		}
		else
		{
			mat.DisableKeyword(keyword);
		}
	}

	public static bool BlendModeIsCutout(this Material mat, float blendMode)
	{
		if (mat.HasProperty("_Mode") && blendMode == 1f)
		{
			mat.SetFloat("_AlphaTest", 1f);
			return true;
		}
		return false;
	}

	public static void SetTogglePropertyWithSetTexture(this Material mat, string toggleProperty, string textureProperty, string keyword)
	{
		if (mat.HasProperty(toggleProperty) && mat.HasProperty(textureProperty))
		{
			bool flag = mat.HasNonNullTexture(textureProperty);
			mat.SetFloat(toggleProperty, flag ? 1 : 0);
			mat.SetKeyword(keyword, flag);
		}
	}
}
