using System;
using GreClient.CardData;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga;

public class CardRolloverZoomHandler : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler, IScrollHandler
{
	[SerializeField]
	private BASE_CDC _cardView;

	[SerializeField]
	public ICardRolloverZoom ZoomView;

	public ICardDataAdapter Card;

	public Collider CardCollider;

	private void OnEnable()
	{
		if (CurrentCamera.Value != null && Physics.Raycast(CurrentCamera.Value.ScreenPointToRay(Input.mousePosition), out var hitInfo) && hitInfo.collider == CardCollider)
		{
			OnPointerEnter(new PointerEventData(null));
		}
	}

	private ICardDataAdapter GetCardData()
	{
		if (Card != null)
		{
			return Card;
		}
		return GetCardView().Model;
	}

	private Bounds GetCardBounds()
	{
		if (CardCollider != null)
		{
			return CardCollider.bounds;
		}
		return GetCardView().GetComponentInChildren<Collider>().bounds;
	}

	private BASE_CDC GetCardView()
	{
		if (_cardView == null)
		{
			_cardView = GetComponentInChildren<BASE_CDC>();
		}
		return _cardView;
	}

	private void HandheldCardViewActivation(ICardDataAdapter adapter)
	{
		ICardRolloverZoom zoomView = ZoomView;
		zoomView.OnRolloverStart = (Action<ICardDataAdapter>)Delegate.Remove(zoomView.OnRolloverStart, new Action<ICardDataAdapter>(HandheldCardViewActivation));
		GetCardView().gameObject.SetActive(value: true);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!eventData.dragging && ZoomView != null)
		{
			ZoomView.CardRolledOver(GetCardData(), GetCardBounds());
			eventData.Use();
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (ZoomView != null)
		{
			ZoomView.CardRolledOff(GetCardData());
			eventData.Use();
		}
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (!eventData.dragging && ZoomView != null)
		{
			ZoomView.CardPointerDown(eventData.button, GetCardData());
			eventData.Use();
		}
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (ZoomView != null)
		{
			ZoomView.CardPointerUp(eventData.button, GetCardData());
			eventData.Use();
		}
	}

	public void OnScroll(PointerEventData eventData)
	{
		if (ZoomView != null && ZoomView.CardScrolled(eventData.scrollDelta))
		{
			eventData.Use();
		}
		else if (base.transform.parent != null)
		{
			base.transform.parent.SendMessageUpwards("OnScroll", eventData, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void EventMouseDown()
	{
		if (ZoomView != null)
		{
			ZoomView.CardPointerDown(PointerEventData.InputButton.Left, GetCardData());
		}
	}

	public void EventMouseUp()
	{
		if (ZoomView != null)
		{
			ZoomView.CardPointerUp(PointerEventData.InputButton.Left, GetCardData());
		}
	}

	public void EventRightClick()
	{
		if (ZoomView != null)
		{
			ZoomView.CardPointerUp(PointerEventData.InputButton.Right, GetCardData());
		}
	}
}
