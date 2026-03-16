using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class FadeAnimationState : CustomButtonAnimationState<CanvasGroup>
{
	[SerializeField]
	private float _value = 1f;

	protected override Tweener OnBegin(CanvasGroup target, float duration)
	{
		return target.DOFade(_value, duration);
	}
}
