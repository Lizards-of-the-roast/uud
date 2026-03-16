using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SceneObjectReference : PropertyAttribute
{
	public Type SceneObjectType;

	public SceneObjectReference(Type sceneObjectType = null)
	{
		SceneObjectType = sceneObjectType ?? typeof(GameObject);
	}

	public static GameObject GetSceneObject(string referencePath)
	{
		if (string.IsNullOrEmpty(referencePath))
		{
			return null;
		}
		if (SceneObjectBeacon.Beacons.TryGetValue(referencePath, out var value))
		{
			return value;
		}
		string[] array = referencePath.Split('/');
		value = null;
		string[] array2 = array;
		foreach (string name in array2)
		{
			value = GetSceneObjectByName(value, name);
		}
		return value;
	}

	public static GameObject[] GetSceneObjects(string referencePath)
	{
		if (string.IsNullOrEmpty(referencePath))
		{
			return null;
		}
		List<GameObject> list = new List<GameObject>();
		foreach (KeyValuePair<string, GameObject> beacon in SceneObjectBeacon.Beacons)
		{
			if (beacon.Key.StartsWith(referencePath))
			{
				list.Add(beacon.Value);
			}
		}
		string[] array = referencePath.Split('/');
		GameObject gameObject = null;
		string[] array2 = array;
		foreach (string name in array2)
		{
			gameObject = GetSceneObjectByName(gameObject, name);
		}
		if (gameObject != null)
		{
			list.Add(gameObject);
		}
		return list.ToArray();
	}

	public static T GetSceneObject<T>(string referencePath) where T : Component
	{
		GameObject sceneObject = GetSceneObject(referencePath);
		if ((object)sceneObject == null)
		{
			return null;
		}
		return sceneObject.GetComponent<T>();
	}

	public static string GetSceneObjectPath(GameObject sceneObject)
	{
		SceneObjectBeacon component = sceneObject.GetComponent<SceneObjectBeacon>();
		if ((bool)component)
		{
			return component.BeaconName;
		}
		string text = GetSceneObjectName(sceneObject);
		while (sceneObject.transform.parent != null)
		{
			sceneObject = sceneObject.transform.parent.gameObject;
			text = GetSceneObjectName(sceneObject) + "/" + text;
		}
		return text;
	}

	private static string GetSceneObjectName(GameObject sceneObject)
	{
		if (sceneObject.transform.parent != null)
		{
			Transform[] array = (from Transform child in sceneObject.transform.parent
				where child.name == sceneObject.name
				select child).ToArray();
			if (array.Length > 1)
			{
				return sceneObject.name + ":" + Array.IndexOf(array, sceneObject.transform);
			}
		}
		return sceneObject.name;
	}

	private static GameObject GetSceneObjectByName(GameObject parent, string name)
	{
		if (parent == null)
		{
			return GameObject.Find(name);
		}
		int result = 0;
		string[] array = name.Split(':');
		if (array.Length > 1)
		{
			int.TryParse(array[1], out result);
		}
		int num = 0;
		foreach (Transform item in parent.transform)
		{
			if (!(item.name != array[0]))
			{
				if (num == result)
				{
					return item.gameObject;
				}
				num++;
			}
		}
		return null;
	}
}
