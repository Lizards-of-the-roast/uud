using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class SizeDeltaAnimationState : CustomButtonAnimationState<RectTransform>
{
	[SerializeField]
	private Vector2 _value = Vector2.zero;

	public Vector2 Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}

	protected override Tweener OnBegin(RectTransform target, float duration)
	{
		return target.DOSizeDelta(_value, duration);
	}
}
