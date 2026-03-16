using System;
using UnityEngine;

public class AccessoryThresholdAnimCycler : MonoBehaviour
{
	[Serializable]
	public class AnimationCycler
	{
		public string clickTriggerName = "";

		public int clicksToNext;
	}

	[TextArea(2, 10)]
	public string toolDescription = "Usage: When an accessory requires a predefined number of click cycle animations\nExample: While playing Click1 animation, register the number of clicks during this state and the next time a click is registered, Click2 happens";

	[Space(10f)]
	[Tooltip("Define Name of Idle State Animation - from which the model should change animations")]
	[SerializeField]
	private string idleAnimationName = "Idle_Loop";

	[Header("Animator Settings")]
	[SerializeField]
	private Animator animator;

	[Tooltip("Use 'Main' unless you have a sub state machine")]
	[SerializeField]
	private string subStateName = "Main";

	[SerializeField]
	private AnimationCycler[] animationCycler;

	private int indexCounter;

	private string animatorStateName;

	private string currentClipName;

	private int currentClickCount;

	private AnimatorClipInfo[] currentClipInfo;

	private float currentClipLength;

	private string currentAnim;

	public void Cycle()
	{
		animatorStateName = subStateName + "." + currentAnim;
		if (animator.GetCurrentAnimatorStateInfo(0).IsName(animatorStateName))
		{
			return;
		}
		currentClickCount++;
		currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
		currentClipName = currentClipInfo[0].clip.name;
		if (currentClickCount >= animationCycler[indexCounter].clicksToNext && currentClipName == idleAnimationName)
		{
			currentAnim = animationCycler[indexCounter].clickTriggerName;
			currentClickCount = 0;
			indexCounter++;
			indexCounter %= animationCycler.Length;
			if (indexCounter == 0)
			{
				currentAnim = "";
			}
		}
		animator.SetTrigger(animationCycler[indexCounter].clickTriggerName);
	}
}
