using UnityEngine;

public static class MaterialUtils
{
	public static void SetToggle(Material mat, string property, bool state)
	{
		mat.SetFloat(property, state ? 1f : 0f);
	}

	public static void SetKeyword(Material mat, string keyword, bool state)
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

	public static void SetKeywordFromProp(Material mat, string prop, string kw)
	{
		if (mat.HasProperty(prop))
		{
			bool state = mat.GetFloat(prop) != 0f;
			SetKeyword(mat, kw, state);
		}
	}

	public static void SetPropFromOnOffKeywords(Material mat, string prop, string kwOn, string kwOff)
	{
		if (mat.HasProperty(prop))
		{
			mat.SetFloat(prop, mat.IsKeywordEnabled(kwOn) ? 1f : 0f);
		}
	}

	public static void SetVectorFromFloatProps(Material mat, string prop, string propX, string propY)
	{
		if (mat.HasProperty(prop) && mat.HasProperty(propX) && mat.HasProperty(propY))
		{
			Vector4 value = new Vector4(mat.GetFloat(propX), mat.GetFloat(propY), 0f, 0f);
			mat.SetVector(prop, value);
		}
	}
}
