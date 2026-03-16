using System;
using System.Linq;
using UnityEngine;

namespace Wotc.Mtga.AutoPlay;

public class AutoPlayComponentGetters
{
	public T FindComponent<T>() where T : Component
	{
		return UnityEngine.Object.FindObjectOfType<T>();
	}

	public Component GetAutoplayHookFromTag(string inTag, Type[] fallbackTypes)
	{
		AutoPlayHook autoPlayHook = Array.Find(UnityEngine.Object.FindObjectsOfType<AutoPlayHook>(), (AutoPlayHook x) => x.AutoPlayTag == inTag);
		if (autoPlayHook == null)
		{
			return FindComponentByPath(inTag, fallbackTypes);
		}
		return autoPlayHook.Target;
	}

	private Component FindComponentByPath(string path, Type[] types)
	{
		GameObject ob = FindTransformByPath(path)?.gameObject;
		if (ob == null)
		{
			return null;
		}
		return types.Select((Type type) => ob.GetComponent(type)).FirstOrDefault((Component component) => component != null);
	}

	private Transform FindTransformByPath(string path)
	{
		string[] array = path.Split('#');
		Transform transform = GameObject.Find(array[0])?.transform;
		for (int i = 1; i < array.Length; i++)
		{
			if (transform == null)
			{
				break;
			}
			if (int.TryParse(array[i], out var result))
			{
				transform = transform.GetChild(result);
				continue;
			}
			string text = array[i];
			foreach (Transform item in transform)
			{
				if (item.name == text)
				{
					transform = item;
					break;
				}
			}
		}
		_ = transform == null;
		return transform;
	}
}
