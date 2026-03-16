using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

public class CardDropHandler : MonoBehaviour, IDropHandler, IEventSystemHandler
{
	public Action<MetaCardView> OnCardDropped;

	public void SetInteractable(bool interactable)
	{
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>(includeInactive: true);
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].enabled = interactable;
		}
		Graphic[] componentsInChildren2 = GetComponentsInChildren<Graphic>(includeInactive: true);
		for (int i = 0; i < componentsInChildren2.Length; i++)
		{
			componentsInChildren2[i].raycastTarget = interactable;
		}
	}

	public void OnDrop(PointerEventData eventData)
	{
		if (eventData.ConfirmOnlyButtonPressed(PointerEventData.InputButton.Left))
		{
			MetaCardView draggingCard = MetaCardUtility.GetDraggingCard(eventData);
			if (draggingCard != null)
			{
				OnCardDropped?.Invoke(draggingCard);
			}
		}
	}
}
