using System;
using Core.Shared.Code.Utilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.Cards.ArtCrops;

[Serializable]
public class ArtCrop
{
	public static readonly ArtCrop DEFAULT = new ArtCrop();

	[JsonConverter(typeof(SlimVectorConverter))]
	public Vector4 ScaleOffset = new Vector4(1f, 1f, 0f, 0f);

	public bool Generated;

	[JsonIgnore]
	public float XScale => ScaleOffset.x;

	[JsonIgnore]
	public float YScale => ScaleOffset.y;

	[JsonIgnore]
	public float XOffset => ScaleOffset.z;

	[JsonIgnore]
	public float YOffset => ScaleOffset.w;

	public void ApplyToMaterial(Material mat)
	{
		ApplyToMaterial(mat, Vector2.zero);
	}

	public void ApplyToMaterial(Material mat, Vector2 manualOffset)
	{
		Vector2 vector = new Vector2(XOffset + manualOffset.x, YOffset + manualOffset.y);
		Vector2 vector2 = new Vector2(XScale, YScale);
		mat.EnableKeyword("_MainTex_SCALE_ON");
		mat.EnableKeyword("_MainTex_OFFSET_ON");
		mat.SetVector(ShaderPropertyIds.TextureScaleOffsetPropId, new Vector4(vector2.x, vector2.y, vector.x, vector.y));
		mat.SetTextureOffset(ShaderPropertyIds.MainTexPropId, vector);
		mat.SetTextureScale(ShaderPropertyIds.MainTexPropId, vector2);
		mat.mainTextureOffset = vector;
		mat.mainTextureScale = vector2;
	}

	public void SetToPropertyBlock(MaterialPropertyBlock matBlock)
	{
		SetToPropertyBlock(matBlock, Vector2.zero);
	}

	public void SetToPropertyBlock(MaterialPropertyBlock matBlock, Vector2 manualOffset)
	{
		Vector2 vector = new Vector2(XOffset + manualOffset.x, YOffset + manualOffset.y);
		Vector2 vector2 = new Vector2(XScale, YScale);
		matBlock.SetVector(ShaderPropertyIds.TextureScaleOffsetPropId, new Vector4(vector2.x, vector2.y, vector.x, vector.y));
		matBlock.SetVector(ShaderPropertyIds.MainTexSTPropId, new Vector4(vector2.x, vector2.y, vector.x, vector.y));
	}

	public void ApplyToUi(RawImage image)
	{
		image.uvRect = new Rect(ScaleOffset.z, ScaleOffset.w, ScaleOffset.x, ScaleOffset.y);
	}
}
