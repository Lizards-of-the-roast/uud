using UnityEngine;

public class DisableOnComplete_SMB : StateMachineBehaviour
{
	public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator, stateInfo, layerIndex);
		GameObject gameObject = animator.gameObject;
		if (gameObject.activeSelf && stateInfo.normalizedTime >= 1f)
		{
			gameObject.SetActive(value: false);
		}
	}

	public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator, stateInfo, layerIndex);
		GameObject gameObject = animator.gameObject;
		if (gameObject.activeSelf)
		{
			gameObject.SetActive(value: false);
		}
	}
}
