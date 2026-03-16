using UnityEngine;

public class CinematicCurtain : MonoBehaviour
{
	[SerializeField]
	public Animator _animator;

	public void Hide()
	{
		if (base.gameObject.activeInHierarchy)
		{
			_animator.SetTrigger("Outro");
		}
	}
}
