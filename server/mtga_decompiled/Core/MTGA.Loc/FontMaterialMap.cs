using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MTGA.Loc;

[Serializable]
[CreateAssetMenu(fileName = "Font_MaterialMap", menuName = "ScriptableObject/Font Material Map")]
public class FontMaterialMap : ScriptableObject
{
	public TMP_FontAsset font;

	public Material[] materials;

	private Dictionary<string, Material> _map;

	private Dictionary<string, Material> lookup
	{
		get
		{
			if (_map == null)
			{
				Init();
			}
			return _map;
		}
	}

	public Material GetMaterial(string key)
	{
		Material value = null;
		if (lookup.TryGetValue(key, out value))
		{
			return value;
		}
		Debug.LogWarning("Font material " + key + " material not found for " + font.name);
		return value;
	}

	private void Init()
	{
		_map = new Dictionary<string, Material>();
		Material[] array = materials;
		foreach (Material material in array)
		{
			string key = material.name.Substring(font.name.Length);
			_map.Add(key, material);
		}
	}
}
