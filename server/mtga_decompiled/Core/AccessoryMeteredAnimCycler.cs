using UnityEngine;

public class AccessoryMeteredAnimCycler : MonoBehaviour
{
	[SerializeField]
	private string cycleName;

	[SerializeField]
	private Animator animator;

	[Tooltip("Use 'Main' unless you have a sub state machine")]
	[SerializeField]
	private string SubStateName = "Main";

	[SerializeField]
	private string[] cycledAnimations;

	private int counter;

	private string currentAnim;

	public void Cycle()
	{
		string text = SubStateName + "." + currentAnim;
		if (!animator.GetCurrentAnimatorStateInfo(0).IsName(text))
		{
			counter %= cycledAnimations.Length;
			animator.SetTrigger(cycledAnimations[counter]);
			currentAnim = cycledAnimations[counter];
			counter++;
		}
	}
}
