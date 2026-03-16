using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using UnityEngine;

namespace Wotc.Mtga.Cards.ArtCrops;

[Serializable]
public class ArtCropFormat
{
	public enum UnsatisfiedBehaviorType
	{
		Ignore,
		Warning,
		Error
	}

	[JsonConverter(typeof(SlimVectorConverter))]
	public Vector2 Dimensions = new Vector2(627f, 459f);

	public float FudgePercentage = 0.01f;

	public UnsatisfiedBehaviorType UnsatisfiedBehavior;

	[JsonIgnore]
	public float Ratio => Dimensions.x / Mathf.Max(1f, Dimensions.y);

	public bool DoesSatisfyFormat(Texture2D artTexture)
	{
		return DoesSatisfyFormat(artTexture, ArtCrop.DEFAULT);
	}

	public bool DoesSatisfyFormat(Texture2D artTexture, ArtCrop crop)
	{
		return DoesSatisfyFormat(artTexture.width, artTexture.height, crop);
	}

	public bool DoesSatisfyFormat(int artWidth, int artHeight, ArtCrop crop)
	{
		float num = (float)artWidth * crop.XScale;
		float b = (float)artHeight * crop.YScale;
		float num2 = Mathf.Abs(num / Mathf.Max(1f, b) - Ratio);
		float num3 = Ratio * FudgePercentage;
		return num2 <= num3;
	}
}
