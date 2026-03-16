using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class RotationAnimationState : CustomButtonAnimationState<RectTransform>
{
	[SerializeField]
	private Vector3 _value = Vector3.zero;

	protected override Tweener OnBegin(RectTransform target, float duration)
	{
		return target.DORotate(_value, duration);
	}
}
