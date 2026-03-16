using System;
using UnityEngine;

public class AccessoryWeightedAnimCycle : MonoBehaviour
{
	[Serializable]
	public class AnimationCycler
	{
		public string clickTriggerName = "";

		[Range(0f, 1f)]
		public float weight;

		public bool hasContinualState;
	}

	[TextArea(2, 10)]
	public string toolDescription = "Usage: When an accessory requires animations weights\nExample: Click1 has more weight, i.e, plays more frequent than Click2";

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

	[Header("Cycler Settings")]
	[SerializeField]
	private AnimationCycler[] animationCycler;

	private string animatorStateName;

	private string currentClipName;

	private AnimatorClipInfo[] currentClipInfo;

	private string currentAnim;

	private bool ignoreWeight;

	private float weight;

	private float totalSum;

	private float randomSum;

	private float weightedSum;

	public void Cycle()
	{
		animatorStateName = subStateName + "." + currentAnim;
		currentClipInfo = animator.GetCurrentAnimatorClipInfo(0);
		currentClipName = currentClipInfo[0].clip.name;
		if (animator.GetCurrentAnimatorStateInfo(0).IsName(animatorStateName))
		{
			return;
		}
		if (animationCycler == null || animationCycler.Length == 0)
		{
			Debug.Log("<color=red>AccessoryWeightedAnimCycle Error:</color> Please assign list of cyclers");
			return;
		}
		if (ignoreWeight)
		{
			if (currentClipName == idleAnimationName)
			{
				ignoreWeight = false;
			}
			else
			{
				animator.SetTrigger(currentAnim);
			}
			return;
		}
		totalSum = 0f;
		for (int i = 0; i < animationCycler.Length; i++)
		{
			weight = animationCycler[i].weight;
			if (float.IsPositiveInfinity(weight))
			{
				Debug.Log("<color=orange>Positive Infinity</color>");
				currentAnim = animationCycler[i].clickTriggerName;
				animator.SetTrigger(animationCycler[i].clickTriggerName);
				if (animationCycler[i].hasContinualState)
				{
					ignoreWeight = true;
				}
				return;
			}
			if (weight >= 0f && !float.IsNaN(weight))
			{
				totalSum += animationCycler[i].weight;
			}
		}
		randomSum = UnityEngine.Random.value;
		weightedSum = 0f;
		for (int j = 0; j < animationCycler.Length; j++)
		{
			weight = animationCycler[j].weight;
			if (float.IsNaN(weight) || weight <= 0f)
			{
				continue;
			}
			weightedSum += weight / totalSum;
			if (weightedSum >= randomSum)
			{
				animator.SetTrigger(animationCycler[j].clickTriggerName);
				currentAnim = animationCycler[j].clickTriggerName;
				if (animationCycler[j].hasContinualState)
				{
					ignoreWeight = true;
				}
				return;
			}
		}
		currentAnim = "";
	}
}
