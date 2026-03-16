using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class ColorAnimationState : CustomButtonAnimationState<Graphic>
{
	[SerializeField]
	private Color _value = Color.white;

	protected override Tweener OnBegin(Graphic target, float duration)
	{
		if (duration <= 0f)
		{
			target.color = _value;
			return null;
		}
		return target.DOColor(_value, duration);
	}
}
