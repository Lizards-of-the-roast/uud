using System.Collections.Generic;
using UnityEngine;

namespace Wizards.Mtga.Utils;

public class GameObjectCore
{
	public static void DestroyComponents<T>(IEnumerable<T> components) where T : Component
	{
		foreach (T component in components)
		{
			Object.Destroy(component.gameObject);
		}
	}
}
