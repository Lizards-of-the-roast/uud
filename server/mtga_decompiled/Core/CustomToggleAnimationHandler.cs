using DG.Tweening;
using UnityEngine;

public abstract class CustomToggleAnimationHandler<TTarget, TState> : MonoBehaviour, ICustomToggleAnimationHandler where TState : CustomButtonAnimationState<TTarget>
{
	[SerializeField]
	private TTarget _target;

	[SerializeField]
	private TState _false;

	[SerializeField]
	private TState _true;

	public void BeginFalse()
	{
		Begin(_false);
	}

	public void BeginFalse(float duration)
	{
		Begin(_false, duration);
	}

	public void BeginTrue()
	{
		Begin(_true);
	}

	public void BeginTrue(float duration)
	{
		Begin(_true, duration);
	}

	private void Begin(TState state)
	{
		if (base.enabled && base.gameObject.activeSelf)
		{
			DOTween.Kill(this);
			state.Begin(_target)?.SetTarget(this);
		}
	}

	private void Begin(TState state, float duration)
	{
		if (base.enabled && base.gameObject.activeSelf)
		{
			DOTween.Kill(this);
			state.Begin(_target, duration)?.SetTarget(this);
		}
	}
}
