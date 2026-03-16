using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneUtils
{
	public static T GetSceneComponent<T>(this Scene scene) where T : Component
	{
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			T component = rootGameObjects[i].GetComponent<T>();
			if (component != null)
			{
				return component;
			}
		}
		return null;
	}

	public static IEnumerable<T> GetSceneComponents<T>(this Scene scene) where T : Component
	{
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootGameObjects.Length; i++)
		{
			if (rootGameObjects[i].TryGetComponent<T>(out var component))
			{
				yield return component;
			}
		}
	}
}
