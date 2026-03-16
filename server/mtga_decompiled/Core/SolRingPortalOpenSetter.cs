using UnityEngine;

public class SolRingPortalOpenSetter : MonoBehaviour
{
	private Animator anim;

	public void Awake()
	{
		anim = GetComponent<Animator>();
	}

	public void OpenPortal()
	{
		anim.SetBool("isPortalOpen", value: true);
	}

	public void ClosePortal()
	{
		anim.SetBool("isPortalOpen", value: false);
	}
}
