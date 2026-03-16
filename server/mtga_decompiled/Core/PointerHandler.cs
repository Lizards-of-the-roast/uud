using UnityEngine;
using UnityEngine.EventSystems;

public class PointerHandler : MonoBehaviour, IBeginDragHandler, IEventSystemHandler, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerUpHandler, IPointerDownHandler
{
	private string cardId = string.Empty;

	private PointerEventData pointerEnter;

	private PointerEventData pointerExit;

	private PointerEventData pointerDown;

	private PointerEventData pointerUp;

	private PointerEventData pointerClick;

	private PointerEventData pointerBeginDrag;

	private PointerEventData pointerDrag;

	private PointerEventData pointerEndDrag;

	public void OnBeginDrag(PointerEventData eventData)
	{
		pointerBeginDrag = eventData;
		pointerDrag = null;
		pointerEndDrag = null;
		Debug.Log("Pointer began drag on " + cardId + " at " + pointerBeginDrag.position.ToString());
	}

	public void OnDrag(PointerEventData eventData)
	{
		pointerDrag = eventData;
		Debug.Log("Pointer dragging on " + cardId + " at " + pointerDrag.position.ToString());
	}

	public void OnEndDrag(PointerEventData eventData)
	{
		pointerEndDrag = eventData;
		Debug.Log("Pointer ended drag on " + cardId + " at " + pointerEndDrag.position.ToString());
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		pointerClick = eventData;
		Debug.Log("Pointer clicked on " + cardId + " at " + pointerClick.position.ToString());
	}

	private void Start()
	{
		cardId = GetInstanceID().ToString();
	}

	private void Update()
	{
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		pointerEnter = eventData;
		Debug.Log("Pointer entering card #" + cardId + " at " + pointerEnter.position.ToString());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		pointerExit = eventData;
		Debug.Log("Pointer exiting card #" + cardId + " at " + pointerExit.position.ToString());
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		pointerUp = eventData;
		Debug.Log("Pointer up on card #" + cardId + " at " + pointerUp.position.ToString());
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		pointerDown = eventData;
		pointerUp = null;
		Debug.Log("Pointer down on card #" + cardId + " at " + pointerDown.position.ToString());
	}
}
