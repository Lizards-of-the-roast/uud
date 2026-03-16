using UnityEngine;

namespace Core.Shared.Code.Utilities;

public static class ShaderPropertyIds
{
	public static readonly int EmissionPropId = Shader.PropertyToID("_Emission");

	public static readonly int EmissionColorPropId = Shader.PropertyToID("_EmissionColor");

	public static readonly int NormalMapPropId = Shader.PropertyToID("_NormalMap");

	public static readonly int BumpMapPropId = Shader.PropertyToID("_BumpMap");

	public static readonly int MainTexPropId = Shader.PropertyToID("_MainTex");

	public static readonly int MainTexSTPropId = Shader.PropertyToID("_MainTex_ST");

	public static readonly int DepthFoilDistTexPropId = Shader.PropertyToID("_DepthFoilDistTex");

	public static readonly int TextureScaleOffsetPropId = Shader.PropertyToID("_TextureScaleOffset");

	public static readonly int ForceUVTogglesOnPropId = Shader.PropertyToID("_ForceUVTogglesOn");

	public static readonly int GlowScalePropId = Shader.PropertyToID("_GlowScale");

	public static readonly int GlowPositionPropId = Shader.PropertyToID("_GlowPosition");

	public static readonly int GrayscaleAmountPropId = Shader.PropertyToID("_GrayscaleAmount");

	public static readonly int ColorPreGrayScalePropId = Shader.PropertyToID("_ColorPreGrayscale");

	public static readonly int ColorPostGrayScalePropId = Shader.PropertyToID("_ColorPostGrayscale");

	public static readonly int RampPropId = Shader.PropertyToID("_Ramp");

	public static readonly int NoisePropId = Shader.PropertyToID("_Noise");

	public static readonly int BrightnessPropId = Shader.PropertyToID("_Brightness");

	public static readonly int AlphaPropId = Shader.PropertyToID("_Alpha");

	public static readonly int SnowLevelPropId = Shader.PropertyToID("_SnowLevel");

	public static readonly int UseDimmedPropId = Shader.PropertyToID("_UseDimmed");

	public static readonly int ViewDirOffsetPropId = Shader.PropertyToID("_ViewDirOffset");

	public static readonly int DissolveAmountPropId = Shader.PropertyToID("_DissolveAmount");

	public static readonly int ModePropId = Shader.PropertyToID("_Mode");

	public static readonly int SrcBlendPropId = Shader.PropertyToID("_SrcBlend");

	public static readonly int DstBlendPropId = Shader.PropertyToID("_DstBlend");

	public static readonly int ZWritePropId = Shader.PropertyToID("_ZWrite");

	public static readonly int CuttingPlanePosPropId = Shader.PropertyToID("_CuttingPlanePos");

	public static readonly int ShirtColorVariantPropId = Shader.PropertyToID("_ShirtColorVariant");

	public static readonly int SkinColorVariantPropId = Shader.PropertyToID("_SkinColorVariant");

	public static readonly int SleevesColorVariantPropId = Shader.PropertyToID("_SleevesColorVariant");

	public static readonly int PantsColorVariantPropId = Shader.PropertyToID("_PantsColorVariant");

	public static readonly int FlagColorVariantPropId = Shader.PropertyToID("_FlagColorVariant");

	public static readonly int ColorPropId = Shader.PropertyToID("_Color");

	public static readonly int LineColorPropId = Shader.PropertyToID("_LineColor");

	public static readonly int MainColorPropId = Shader.PropertyToID("_MainColor");

	public static readonly int MainColor2PropId = Shader.PropertyToID("_MainColor2");

	public static readonly int BloomColorPropId = Shader.PropertyToID("_Bloom_Color");

	public static readonly int AnimOffsetPropId = Shader.PropertyToID("_AnimOffset");

	public static readonly int AnimTexIndexPropId = Shader.PropertyToID("_AnimTexIndex");

	public static readonly int SampleScalePropId = Shader.PropertyToID("_SampleScale");
}
