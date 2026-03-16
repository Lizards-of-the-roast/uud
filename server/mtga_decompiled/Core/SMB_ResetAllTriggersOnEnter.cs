using UnityEngine;

public class SMB_ResetAllTriggersOnEnter : StateMachineBehaviour
{
	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		for (int i = 0; i < animator.parameterCount; i++)
		{
			if (animator.parameters[i].type == AnimatorControllerParameterType.Trigger)
			{
				animator.ResetTrigger(animator.parameters[i].name);
			}
		}
	}
}
