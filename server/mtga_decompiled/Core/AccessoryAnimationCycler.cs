using UnityEngine;

public class AccessoryAnimationCycler : MonoBehaviour
{
	[SerializeField]
	private string cycleName;

	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string[] cycledAnimations;

	private int counter;

	public void Cycle()
	{
		counter %= cycledAnimations.Length;
		animator.SetTrigger(cycledAnimations[counter]);
		counter++;
	}
}
