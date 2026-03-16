using System;
using System.Collections.Generic;
using UnityEngine;

public class Battlefield_RandomSpawner : MonoBehaviour
{
	public float velocityMin;

	public float velocityMax;

	public GameObject SpawnPointGroup;

	public List<GameObject> objectCatalog = new List<GameObject>();

	private void Start()
	{
		if (!SpawnPointGroup)
		{
			return;
		}
		Transform[] componentsInChildren = SpawnPointGroup.GetComponentsInChildren<Transform>();
		List<GameObject> list = new List<GameObject>();
		Transform[] array = componentsInChildren;
		foreach (Transform transform in array)
		{
			list.Add(transform.gameObject);
		}
		System.Random random = new System.Random();
		foreach (GameObject item in list)
		{
			int index = random.Next(objectCatalog.Count);
			GameObject obj = UnityEngine.Object.Instantiate(objectCatalog[index], item.transform.position, Quaternion.identity);
			obj.transform.parent = item.transform;
			obj.transform.localScale = Vector3.one;
			(obj.AddComponent(typeof(Battlefield_SpawnedObject)) as Battlefield_SpawnedObject).velocity = UnityEngine.Random.Range(velocityMin, velocityMax);
		}
	}
}
