using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class StoreItemBaseDraggableEventTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerClickHandler
{
	public UnityEvent PointerEnter;

	public UnityEvent PointerExit;

	public UnityEvent PointerClick;

	public void OnPointerEnter(PointerEventData eventData)
	{
		PointerEnter.Invoke();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		PointerExit.Invoke();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		PointerClick.Invoke();
	}
}
