using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Wotc.Mtga.CustomInput;

public class BattlefieldSecondaryRaycaster : MonoBehaviour
{
	[SerializeField]
	[Tooltip("Interact with objects on these layers")]
	private LayerMask _interactiveLayers;

	[SerializeField]
	[Tooltip("Allow objects on these layers to block interaction")]
	private LayerMask _blockingLayers;

	private HashSet<IEventSystemHandler> _pointerHandlersCurrent = new HashSet<IEventSystemHandler>();

	private HashSet<IEventSystemHandler> _pointerHandlersPrevious = new HashSet<IEventSystemHandler>();

	private void Update()
	{
		if (!CurrentCamera.Value)
		{
			return;
		}
		HashSet<IEventSystemHandler> pointerHandlersPrevious = _pointerHandlersPrevious;
		_pointerHandlersPrevious = _pointerHandlersCurrent;
		_pointerHandlersCurrent = pointerHandlersPrevious;
		_pointerHandlersCurrent.Clear();
		PointerEventData pointerEventData = CustomInputModule.GetLastPointerEventData();
		Vector3 vector = ((Input.touchCount > 0) ? ((Vector3)Input.GetTouch(0).position) : Input.mousePosition);
		if (CustomInputModule.IsOutOfScreenBounds(vector, Screen.width, Screen.height))
		{
			return;
		}
		if (Physics.Raycast(CurrentCamera.Value.ScreenPointToRay(vector), out var hitInfo, float.PositiveInfinity, (int)_interactiveLayers | (int)_blockingLayers) && ((1 << hitInfo.collider.gameObject.layer) & (int)_blockingLayers) < 1 && (EventSystem.current == null || !EventSystem.current.IsPointerOverGameObject() || !CustomInputModule.GetHovered().Any((GameObject x) => (bool)x && x.TryGetComponent<Graphic>(out var _))))
		{
			pointerEventData = new PointerEventData(EventSystem.current)
			{
				pointerPressRaycast = new RaycastResult
				{
					gameObject = hitInfo.collider.gameObject
				},
				rawPointerPress = hitInfo.collider.gameObject,
				pointerCurrentRaycast = new RaycastResult
				{
					worldPosition = hitInfo.point
				}
			};
			pointerEventData.hovered.Add(hitInfo.collider.gameObject);
			IPointerEnterHandler componentInParent = hitInfo.collider.gameObject.GetComponentInParent<IPointerEnterHandler>();
			if (componentInParent != null)
			{
				_pointerHandlersCurrent.Add(componentInParent);
				if (!_pointerHandlersPrevious.Contains(componentInParent))
				{
					componentInParent.OnPointerEnter(pointerEventData);
				}
			}
			IPointerExitHandler componentInParent2 = hitInfo.collider.gameObject.GetComponentInParent<IPointerExitHandler>();
			if (componentInParent2 != null)
			{
				_pointerHandlersCurrent.Add(componentInParent2);
			}
			if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
			{
				hitInfo.collider.gameObject.GetComponentInParent<IPointerDownHandler>()?.OnPointerDown(pointerEventData);
			}
			if (Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended))
			{
				hitInfo.collider.gameObject.GetComponentInParent<IPointerClickHandler>()?.OnPointerClick(pointerEventData);
			}
		}
		if (pointerEventData == null)
		{
			return;
		}
		foreach (IEventSystemHandler pointerHandlersPreviou in _pointerHandlersPrevious)
		{
			if (!_pointerHandlersCurrent.Contains(pointerHandlersPreviou) && pointerHandlersPreviou is IPointerExitHandler pointerExitHandler)
			{
				pointerExitHandler.OnPointerExit(pointerEventData);
			}
		}
	}
}
