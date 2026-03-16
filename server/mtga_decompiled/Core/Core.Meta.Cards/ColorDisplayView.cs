using System;
using System.Collections.Generic;
using UnityEngine;

namespace Core.Meta.Cards;

public class ColorDisplayView : MonoBehaviour
{
	[Serializable]
	public class ColorDisplayInfo
	{
		[Tooltip("A prefab for an icon to display.")]
		public GameObject IconPrefab;

		[Tooltip("A list of keys to use to look up the given prefab.")]
		public string[] Keys;
	}

	private List<GameObject> _spawnedIcons = new List<GameObject>();

	[SerializeField]
	private List<ColorDisplayInfo> _iconPrefabs;

	private Dictionary<string, ColorDisplayInfo> _iconPrefabsMap = new Dictionary<string, ColorDisplayInfo>(StringComparer.OrdinalIgnoreCase);

	[SerializeField]
	private Transform container;

	public void SetColors(string[] colors)
	{
		Clear();
		foreach (string color in colors)
		{
			SpawnIcon(color);
		}
	}

	private void Awake()
	{
		if (container == null)
		{
			container = base.transform;
		}
		foreach (ColorDisplayInfo iconPrefab in _iconPrefabs)
		{
			string[] keys = iconPrefab.Keys;
			foreach (string key in keys)
			{
				_iconPrefabsMap[key] = iconPrefab;
			}
		}
	}

	private void SpawnIcon(string color)
	{
		if (_iconPrefabsMap.TryGetValue(color, out var value) && value.IconPrefab != null)
		{
			GameObject item = UnityEngine.Object.Instantiate(value.IconPrefab, container);
			_spawnedIcons.Add(item);
		}
	}

	private void Clear()
	{
		foreach (GameObject spawnedIcon in _spawnedIcons)
		{
			UnityEngine.Object.Destroy(spawnedIcon);
		}
		_spawnedIcons.Clear();
	}
}
