using UnityEngine;

namespace EventPage.Components;

public abstract class EventComponent : EventPageLayoutObject
{
	[Header("Event Component")]
	[SerializeField]
	protected Animator _transitionAnimator;

	protected virtual Animator Animator
	{
		get
		{
			if (_transitionAnimator == null)
			{
				_transitionAnimator = base.gameObject.GetComponent<Animator>();
			}
			return _transitionAnimator;
		}
	}

	public virtual void PlayAnimation(string animation)
	{
		if (!(Animator != null))
		{
			return;
		}
		if (!(animation == "moduleIntro"))
		{
			if (animation == "moduleOutro")
			{
				Animator.SetTrigger("Outro");
			}
		}
		else
		{
			Animator.SetTrigger("Intro");
		}
	}
}
