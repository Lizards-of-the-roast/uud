using DG.Tweening;
using UnityEngine;

public abstract class CustomButtonAnimationHandler<TTarget, TState> : MonoBehaviour, ICustomButtonAnimationHandler where TState : CustomButtonAnimationState<TTarget>
{
	[SerializeField]
	private TTarget _target;

	[SerializeField]
	private TState _disabled;

	[SerializeField]
	private TState _mouseOff;

	[SerializeField]
	private TState _mouseOn;

	[SerializeField]
	private TState _pressedOn;

	[SerializeField]
	private TState _pressedOff;

	public TState Disabled => _disabled;

	public TState MouseOff => _mouseOff;

	public TState MouseOver => _mouseOn;

	public TState PressedOver => _pressedOn;

	public TState PressedOff => _pressedOff;

	public void BeginDisabled()
	{
		Begin(_disabled);
	}

	public void BeginDisabled(float duration)
	{
		Begin(_disabled, duration);
	}

	public void BeginMouseOff()
	{
		Begin(_mouseOff);
	}

	public void BeginMouseOff(float duration)
	{
		Begin(_mouseOff, duration);
	}

	public void BeginMouseOver()
	{
		Begin(_mouseOn);
	}

	public void BeginMouseOver(float duration)
	{
		Begin(_mouseOn, duration);
	}

	public void BeginPressedOver()
	{
		Begin(_pressedOn);
	}

	public void BeginPressedOver(float duration)
	{
		Begin(_pressedOn, duration);
	}

	public void BeginPressedOff()
	{
		Begin(_pressedOff);
	}

	public void BeginPressedOff(float duration)
	{
		Begin(_pressedOff, duration);
	}

	private void Begin(TState state)
	{
		if (base.enabled && base.gameObject.activeSelf)
		{
			DOTween.Kill(this);
			state.Begin(_target).SetTarget(this);
		}
	}

	private void Begin(TState state, float duration)
	{
		if (base.enabled && base.gameObject.activeSelf)
		{
			DOTween.Kill(this);
			state.Begin(_target, duration).SetTarget(this);
		}
	}
}
