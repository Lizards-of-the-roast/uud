using UnityEngine;

namespace Wotc.Mtga.Extensions;

public static class GameObjectExtensions
{
	public static void UpdateActive(this GameObject go, bool active, bool recursive = false)
	{
		if (!go)
		{
			return;
		}
		if (go.activeSelf != active)
		{
			go.SetActive(active);
		}
		if (recursive)
		{
			Transform transform = go.transform;
			for (int i = 0; i < transform.childCount; i++)
			{
				transform.GetChild(i).gameObject.UpdateActive(active, recursive);
			}
		}
	}

	public static T AddOrGetComponent<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (val == null)
		{
			val = go.AddComponent<T>();
		}
		return val;
	}

	public static void SetLayer(this GameObject obj, int newLayer, bool includeChildren = true)
	{
		obj.layer = newLayer;
		if (!includeChildren)
		{
			return;
		}
		int childCount = obj.transform.childCount;
		for (int i = 0; i < childCount; i++)
		{
			Transform child = obj.transform.GetChild(i);
			if (!(child == null))
			{
				child.gameObject.SetLayer(newLayer);
			}
		}
	}

	public static T GetComponentInChildrenEvenIfInactive<T>(this GameObject go) where T : Component
	{
		T val = go.GetComponent<T>();
		if (!val)
		{
			Transform component = go.GetComponent<Transform>();
			for (int i = 0; i < component.childCount; i++)
			{
				val = component.GetChild(i).gameObject.GetComponentInChildrenEvenIfInactive<T>();
				if ((bool)val)
				{
					break;
				}
			}
		}
		return val;
	}
}
