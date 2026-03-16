using System;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class AnimationCompleteTrigger : MonoBehaviour
{
	public UnityEvent OnAnimationComplete;

	private AnimationComplete_SMB _animationComplete;

	private void OnEnable()
	{
		_animationComplete = GetComponent<Animator>().GetBehaviour<AnimationComplete_SMB>();
		if (_animationComplete != null)
		{
			AnimationComplete_SMB animationComplete = _animationComplete;
			animationComplete.OnAnimationComplete = (Action)Delegate.Combine(animationComplete.OnAnimationComplete, new Action(OnAnimationComplete.Invoke));
		}
	}

	private void OnDisable()
	{
		if (_animationComplete != null)
		{
			AnimationComplete_SMB animationComplete = _animationComplete;
			animationComplete.OnAnimationComplete = (Action)Delegate.Remove(animationComplete.OnAnimationComplete, new Action(OnAnimationComplete.Invoke));
		}
	}
}
