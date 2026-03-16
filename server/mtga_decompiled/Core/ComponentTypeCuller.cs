using System.Collections.Generic;
using UnityEngine;

public class ComponentTypeCuller : MonoBehaviour
{
	private GameObject[] objects;

	private ParticleSystem[] particleSystems;

	public bool vfx = true;

	public bool meshes = true;

	private void Start()
	{
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in base.transform)
		{
			list.Add(item.transform.gameObject);
		}
		objects = list.ToArray();
		if (!vfx)
		{
			GameObject[] array = objects;
			for (int i = 0; i < array.Length; i++)
			{
				_ = array[i];
				ParticleSystem[] componentsInChildren = GetComponentsInChildren<ParticleSystem>(includeInactive: true);
				for (int j = 0; j < componentsInChildren.Length; j++)
				{
					componentsInChildren[j].gameObject.SetActive(value: false);
				}
			}
		}
		if (!meshes)
		{
			MeshRenderer[] componentsInChildren2 = GetComponentsInChildren<MeshRenderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren2.Length; i++)
			{
				componentsInChildren2[i].gameObject.SetActive(value: false);
			}
			SkinnedMeshRenderer[] componentsInChildren3 = GetComponentsInChildren<SkinnedMeshRenderer>(includeInactive: true);
			for (int i = 0; i < componentsInChildren3.Length; i++)
			{
				componentsInChildren3[i].gameObject.SetActive(value: false);
			}
		}
	}
}
