using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class ScaleAnimationState : CustomButtonAnimationState<RectTransform>
{
	[SerializeField]
	private float _value;

	protected override Tweener OnBegin(RectTransform target, float duration)
	{
		return target.DOScale(_value, duration);
	}
}
