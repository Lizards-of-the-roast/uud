using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga;

public class CardZoomTrigger : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public bool DisableCardOnZoom = true;

	public BASE_CDC CardView { get; set; }

	public ICardRolloverZoom ZoomView { get; set; }

	private bool HasCardView
	{
		get
		{
			if (CardView != null)
			{
				return CardView.gameObject != null;
			}
			return false;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!eventData.dragging && ZoomView != null && HasCardView)
		{
			if (ZoomView.CardRolledOver(CardView.Model, CardView.GetComponentInChildren<Collider>().bounds) && DisableCardOnZoom)
			{
				CardView.gameObject.SetActive(value: false);
			}
			eventData.Use();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (ZoomView != null && HasCardView)
		{
			ZoomView.CardRolledOff(CardView.Model);
			CardView.gameObject.SetActive(value: true);
			eventData.Use();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!eventData.dragging && ZoomView != null && HasCardView)
		{
			ZoomView.CardPointerDown(eventData.button, CardView.Model);
			eventData.Use();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (ZoomView != null && HasCardView)
		{
			ZoomView.CardPointerUp(eventData.button, CardView.Model);
			eventData.Use();
		}
	}
}
