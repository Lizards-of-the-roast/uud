using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace Wotc.Mtga.CustomInput;

public class CustomUIInputModule : InputSystemUIInputModule
{
	protected override void Awake()
	{
		base.Awake();
		base.pointerBehavior = UIPointerBehavior.SingleUnifiedPointer;
	}

	public static IEnumerable<GameObject> GetHovered()
	{
		EventSystem current = EventSystem.current;
		if ((object)current == null || !(current.currentInputModule is CustomUIInputModule customUIInputModule))
		{
			yield break;
		}
		RaycastResult lastRaycastResult = customUIInputModule.GetLastRaycastResult(0);
		if ((bool)lastRaycastResult.gameObject)
		{
			Transform stackTransform = lastRaycastResult.gameObject.transform;
			while ((bool)stackTransform)
			{
				yield return stackTransform.gameObject;
				stackTransform = stackTransform.parent;
			}
		}
	}

	public static PointerEventData GetLastPointerEventData()
	{
		PointerEventData pointerEventData = null;
		EventSystem current = EventSystem.current;
		if ((object)current != null && current.currentInputModule is CustomUIInputModule customUIInputModule)
		{
			pointerEventData = new PointerEventData(EventSystem.current);
			RaycastResult pointerCurrentRaycast = (pointerEventData.pointerPressRaycast = customUIInputModule.GetLastRaycastResult(0));
			pointerEventData.rawPointerPress = pointerCurrentRaycast.gameObject;
			pointerEventData.pointerCurrentRaycast = pointerCurrentRaycast;
			GameObject gameObject = pointerEventData.pointerCurrentRaycast.gameObject;
			if ((bool)gameObject)
			{
				Transform parent = gameObject.transform;
				while ((bool)parent)
				{
					pointerEventData.hovered.Add(parent.gameObject);
					parent = parent.parent;
				}
			}
		}
		return pointerEventData;
	}
}
