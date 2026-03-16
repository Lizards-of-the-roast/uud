using System;
using DG.Tweening;
using UnityEngine;

[Serializable]
public class LocalMoveAnimationState : CustomButtonAnimationState<Transform>
{
	[SerializeField]
	private Vector3 _value = Vector3.zero;

	protected override Tweener OnBegin(Transform target, float duration)
	{
		return target.DOLocalMove(_value, duration);
	}
}
