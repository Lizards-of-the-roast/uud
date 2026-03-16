using UnityEngine;
using UnityEngine.EventSystems;

public class BirdBehavior : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public Animator animator;

	private void Start()
	{
	}

	private void Update()
	{
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		animator.SetTrigger("TakeOff");
	}
}
