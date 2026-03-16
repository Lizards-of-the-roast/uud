using UnityEngine;

public class SMB_SetUninterruptableBool : StateMachineBehaviour
{
	public bool Uninterruptable = true;

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		AnimatorControllerParameter[] parameters = animator.parameters;
		foreach (AnimatorControllerParameter animatorControllerParameter in parameters)
		{
			if (animatorControllerParameter.name == "Uninterruptable" && animatorControllerParameter.type == AnimatorControllerParameterType.Bool)
			{
				animator.SetBool("Uninterruptable", Uninterruptable);
				break;
			}
		}
	}
}
