using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardZoomModalFade : MonoBehaviour, IPointerUpHandler, IEventSystemHandler
{
	public Action OnClicked;

	[Tooltip("Amount of drag allowed as a fraction of screen height before disallowing OnClick to fire upon pointer release")]
	[SerializeField]
	private float _screenDragThreshold = 0.1f;

	public void OnPointerUp(PointerEventData eventData)
	{
		if (Vector3.Distance(eventData.pressPosition, eventData.position) < _screenDragThreshold * (float)Screen.height)
		{
			OnClicked?.Invoke();
		}
	}
}
