using UnityEngine;
using UnityEngine.EventSystems;

public class LocalHandClickBlocker : MonoBehaviour, IPointerClickHandler, IEventSystemHandler, IPointerDownHandler
{
	private HandCardHolder _cardHolder;

	private void Start()
	{
		_cardHolder = GetComponentInParent<HandCardHolder>();
	}

	void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
	{
		_cardHolder.SetHandCollapse(collapsed: true);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		_cardHolder.OnPointerEnter(null);
	}
}
