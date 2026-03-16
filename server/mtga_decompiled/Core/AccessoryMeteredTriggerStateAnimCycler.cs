using UnityEngine;

public class AccessoryMeteredTriggerStateAnimCycler : MonoBehaviour
{
	[SerializeField]
	private Animator animator;

	[Header("Clicks will be registered during the following trigger states")]
	[SerializeField]
	private string[] triggerAcceptingStates;

	[SerializeField]
	private string[] cycledAnimations;

	private int counter;

	public void Cycle()
	{
		string[] array = triggerAcceptingStates;
		foreach (string text in array)
		{
			if (text != null && animator.GetCurrentAnimatorStateInfo(0).IsName(text))
			{
				counter %= cycledAnimations.Length;
				if (!animator.GetCurrentAnimatorStateInfo(0).IsName(cycledAnimations[counter]))
				{
					animator.SetTrigger(cycledAnimations[counter]);
				}
			}
		}
	}
}
