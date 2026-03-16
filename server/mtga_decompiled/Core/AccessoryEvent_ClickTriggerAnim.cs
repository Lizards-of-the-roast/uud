using UnityEngine;
using UnityEngine.EventSystems;

public class AccessoryEvent_ClickTriggerAnim : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	[SerializeField]
	private Animator animator;

	[SerializeField]
	private string trigger;

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		animator.SetTrigger(trigger);
	}
}
