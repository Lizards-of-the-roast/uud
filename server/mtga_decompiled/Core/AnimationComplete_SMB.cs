using System;
using UnityEngine;
using UnityEngine.Events;

public class AnimationComplete_SMB : StateMachineBehaviour
{
	public Action OnAnimationComplete;

	public UnityEvent OnComplete;

	public bool CompleteOnExit;

	public bool DisableOnComplete;

	private bool _completed;

	private Animator _animator;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		_animator = animator;
		_completed = false;
		if (!CompleteOnExit)
		{
			CompleteAnimation();
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		CompleteAnimation();
	}

	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (stateInfo.normalizedTime >= 1f)
		{
			CompleteAnimation();
		}
	}

	private void CompleteAnimation()
	{
		if (!_completed)
		{
			_completed = true;
			OnAnimationComplete?.Invoke();
			OnComplete?.Invoke();
			if (DisableOnComplete && _animator.gameObject.activeSelf)
			{
				_animator.gameObject.SetActive(value: false);
			}
		}
	}

	public void Reset()
	{
		_completed = false;
	}
}
