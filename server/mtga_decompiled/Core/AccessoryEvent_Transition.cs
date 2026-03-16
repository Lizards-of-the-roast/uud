using UnityEngine;

public class AccessoryEvent_Transition : MonoBehaviour
{
	public AccessoryVariantDelegate_Phases controller;

	private Animator animator;

	private void Start()
	{
		animator = GetComponent<Animator>();
	}

	private void IsTransitioning(int i)
	{
		bool value = i > 0;
		animator.SetBool("IsTransitioning", value);
	}

	public void TransitionWarmUp()
	{
		controller.TransitionWarmUp();
		IsTransitioning(1);
	}

	public void TransitionExecute()
	{
		controller.TransitionExecute();
	}

	public void TransitionEnd()
	{
		controller.TransitionEnd();
		IsTransitioning(0);
	}
}
