using Core.Shared.Code.Utilities;
using DG.Tweening;
using UnityEngine;

public static class GrayscaleShader
{
	public static void SetGrayscale(Material material, float percent)
	{
		material.SetFloat(ShaderPropertyIds.GrayscaleAmountPropId, Mathf.Clamp01(percent));
	}

	public static Tweener AnimateGrayscale(Material material, float percent, float duration)
	{
		return DOTween.To(() => material.GetFloat(ShaderPropertyIds.GrayscaleAmountPropId), delegate(float x)
		{
			material.SetFloat(ShaderPropertyIds.GrayscaleAmountPropId, x);
		}, Mathf.Clamp01(percent), duration);
	}

	public static void SetTintPreGrayscale(Material material, Color color)
	{
		material.SetColor(ShaderPropertyIds.ColorPreGrayScalePropId, color);
	}

	public static Tweener AnimateTintPreGrayscale(Material material, Color color, float duration)
	{
		return DOTween.To(() => material.GetColor(ShaderPropertyIds.ColorPreGrayScalePropId), delegate(Color x)
		{
			material.SetColor(ShaderPropertyIds.ColorPreGrayScalePropId, x);
		}, color, duration);
	}

	public static void SetTintPostGrayscale(Material material, Color color)
	{
		material.SetColor(ShaderPropertyIds.ColorPostGrayScalePropId, color);
	}

	public static Tweener AnimateTintPostGrayscale(Material material, Color color, float duration)
	{
		return DOTween.To(() => material.GetColor(ShaderPropertyIds.ColorPostGrayScalePropId), delegate(Color x)
		{
			material.SetColor(ShaderPropertyIds.ColorPostGrayScalePropId, x);
		}, color, duration);
	}
}
