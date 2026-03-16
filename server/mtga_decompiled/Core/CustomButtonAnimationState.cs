using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public abstract class CustomButtonAnimationState<TTarget>
{
	[SerializeField]
	private float _delay;

	[SerializeField]
	private float _duration = 0.25f;

	[SerializeField]
	private Ease _ease = Ease.InOutSine;

	public Tweener Begin(TTarget target)
	{
		return Begin(target, _duration);
	}

	public Tweener Begin(TTarget target, float duration)
	{
		Tweener tweener = OnBegin(target, duration);
		if (tweener != null)
		{
			tweener.SetDelay(_delay);
			tweener.SetEase(_ease);
		}
		return tweener;
	}

	protected abstract Tweener OnBegin(TTarget target, float duration);
}
