using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public static class MetaCardUtility
{
	public static MetaCardView GetDraggingCard(PointerEventData eventData)
	{
		if (!(eventData.pointerDrag != null))
		{
			return null;
		}
		return eventData.pointerDrag.GetComponent<MetaCardView>();
	}

	public static bool RaycastIsPointerOver(this Transform parent)
	{
		PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
		pointerEventData.position = Input.mousePosition;
		List<RaycastResult> list = new List<RaycastResult>();
		EventSystem.current.RaycastAll(pointerEventData, list);
		return list.FindIndex((RaycastResult r) => parent.IsObjInChildren(r.gameObject.transform)) >= 0;
	}

	public static bool IsObjInChildren(this Transform parent, Transform obj)
	{
		while (obj != null)
		{
			if (obj == parent)
			{
				return true;
			}
			obj = obj.parent;
		}
		return false;
	}
}
