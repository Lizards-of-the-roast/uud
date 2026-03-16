using UnityEngine;
using UnityEngine.EventSystems;

public class ForwardClicksToChildren : MonoBehaviour, IPointerClickHandler, IEventSystemHandler
{
	public void Start()
	{
	}

	public void OnPointerClick(PointerEventData pointerEventData)
	{
		foreach (Transform item in base.transform)
		{
			ExecuteEvents.Execute(item.gameObject, pointerEventData, ExecuteEvents.pointerClickHandler);
		}
	}
}
