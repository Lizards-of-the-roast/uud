using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Cards;

public class ViewAbilityBadge : MonoBehaviour
{
	[SerializeField]
	private SpriteRenderer _spriteRenderer;

	[SerializeField]
	private SpriteRenderer _highlightRenderer;

	[SerializeField]
	private TMP_Text _label;

	private bool _materialsInit;

	private readonly AssetLoader.AssetTracker<Sprite> _spriteTracker = new AssetLoader.AssetTracker<Sprite>("ViewAbilityBadgeSprite");

	public Dictionary<AltReferencedMaterialAndBlock, MaterialReplacementData> MaterialReplacementMappings { get; private set; } = new Dictionary<AltReferencedMaterialAndBlock, MaterialReplacementData>();

	public void Init()
	{
		if (!_materialsInit)
		{
			_materialsInit = true;
			MaterialReplacementMappings = GetMaterialMappings();
			return;
		}
		foreach (KeyValuePair<AltReferencedMaterialAndBlock, MaterialReplacementData> materialReplacementMapping in MaterialReplacementMappings)
		{
			materialReplacementMapping.Value.UpdateOverride(materialReplacementMapping.Key);
			materialReplacementMapping.Value.Reset();
		}
	}

	public void SetSprite(string spritePath)
	{
		_spriteRenderer.sprite = _spriteTracker.Acquire(spritePath);
	}

	public void SetHighlight(bool enabled)
	{
		_highlightRenderer.enabled = enabled;
	}

	public void SetText(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		_label.SetText(text);
	}

	private Dictionary<AltReferencedMaterialAndBlock, MaterialReplacementData> GetMaterialMappings()
	{
		Dictionary<AltReferencedMaterialAndBlock, MaterialReplacementData> dictionary = new Dictionary<AltReferencedMaterialAndBlock, MaterialReplacementData>();
		Dictionary<Material, HashSet<Renderer>> allMaterialsOnObject = new Dictionary<Material, HashSet<Renderer>>();
		GatherFromObjectAndChildren(base.gameObject);
		foreach (KeyValuePair<Material, HashSet<Renderer>> item in allMaterialsOnObject)
		{
			AltReferencedMaterialAndBlock altReferencedMaterialAndBlock = new AltReferencedMaterialAndBlock(item.Key);
			dictionary[altReferencedMaterialAndBlock] = new MaterialReplacementData(altReferencedMaterialAndBlock, GetMaterialIndicesFromMeshRenderers(item.Value, item.Key.name), MaterialOverrideType.Generic);
		}
		return dictionary;
		void GatherFromObjectAndChildren(GameObject gameObject)
		{
			if (!gameObject.GetComponent<TMP_Text>() && !gameObject.GetComponent<TMP_SubMesh>() && !gameObject.GetComponent<ParticleSystem>())
			{
				Renderer component = gameObject.GetComponent<Renderer>();
				if ((bool)component)
				{
					Material[] sharedMaterials = component.sharedMaterials;
					foreach (Material material in sharedMaterials)
					{
						if ((bool)material)
						{
							if (!allMaterialsOnObject.TryGetValue(material, out var value))
							{
								value = (allMaterialsOnObject[material] = new HashSet<Renderer>());
							}
							value.Add(component);
						}
					}
				}
			}
			Transform transform = gameObject.transform;
			for (int j = 0; j < transform.childCount; j++)
			{
				GatherFromObjectAndChildren(transform.GetChild(j).gameObject);
			}
		}
	}

	private List<MeshMaterialIndex> GetMaterialIndicesFromMeshRenderers(HashSet<Renderer> targetRoots, string materialName)
	{
		List<MeshMaterialIndex> list = new List<MeshMaterialIndex>();
		foreach (Renderer targetRoot in targetRoots)
		{
			Material[] sharedMaterials = targetRoot.sharedMaterials;
			for (int i = 0; i < sharedMaterials.Length; i++)
			{
				Material material = sharedMaterials[i];
				if ((bool)material && string.Equals(material.name.Replace("(Instance)", string.Empty).Replace("(Clone)", string.Empty).TrimEnd(), materialName, StringComparison.InvariantCultureIgnoreCase))
				{
					list.Add(new MeshMaterialIndex
					{
						Renderer = targetRoot,
						Index = i
					});
				}
			}
		}
		return list;
	}

	public void Cleanup()
	{
		_spriteRenderer.sprite = null;
		_spriteTracker.Cleanup();
	}

	public void OnDestroy()
	{
		Cleanup();
	}
}
