using UnityEngine;

public class DoomWhisperer_ClipSet : MonoBehaviour
{
	public enum Clips
	{
		One,
		Two
	}

	private Animator animator;

	public float Booms;

	public Clips Clip;

	private void Start()
	{
		animator = GetComponent<Animator>();
		switch (Clip)
		{
		case Clips.One:
			animator.SetBool("1", value: true);
			animator.SetBool("2", value: false);
			break;
		case Clips.Two:
			animator.SetBool("1", value: false);
			animator.SetBool("2", value: true);
			break;
		}
	}
}
