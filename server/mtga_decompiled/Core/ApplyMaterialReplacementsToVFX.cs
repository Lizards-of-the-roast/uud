using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.VfxSettings;
using GreClient.CardData;
using UnityEngine;

public class ApplyMaterialReplacementsToVFX : MonoBehaviour
{
	private readonly Dictionary<Renderer, Material[]> _originalMaterials = new Dictionary<Renderer, Material[]>();

	private readonly HashSet<TextureOverride> _texturePayloadsCache = new HashSet<TextureOverride>();

	private readonly List<AltReferencedMaterial> _textureLoaders = new List<AltReferencedMaterial>();

	[SerializeField]
	private LUTVFXType _lutVFXType = LUTVFXType.NA;

	private void Awake()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		foreach (Renderer renderer in componentsInChildren)
		{
			_originalMaterials.Add(renderer, renderer.materials);
		}
	}

	private void OnDisable()
	{
		foreach (KeyValuePair<Renderer, Material[]> originalMaterial in _originalMaterials)
		{
			if (!(originalMaterial.Key == null))
			{
				originalMaterial.Key.materials = originalMaterial.Value;
			}
		}
		FreeLoadedTextures();
	}

	public void Apply(ICardDataAdapter model, AssetLookupSystem assetLookupSystem)
	{
		FreeLoadedTextures();
		if (assetLookupSystem.TreeLoader == null || !assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<TextureOverride> loadedTree))
		{
			return;
		}
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.SetCardDataExtensive(model);
		assetLookupSystem.Blackboard.LutVfxType = _lutVFXType;
		foreach (KeyValuePair<Renderer, Material[]> originalMaterial in _originalMaterials)
		{
			if (originalMaterial.Key == null)
			{
				continue;
			}
			bool flag = false;
			Material[] materials = originalMaterial.Key.materials;
			for (int i = 0; i < originalMaterial.Value.Length; i++)
			{
				Material material = originalMaterial.Value[i];
				if (!material)
				{
					continue;
				}
				assetLookupSystem.Blackboard.Material = material;
				assetLookupSystem.Blackboard.Texture = material.mainTexture as Texture2D;
				if (!loadedTree.GetPayloadLayered(assetLookupSystem.Blackboard, _texturePayloadsCache))
				{
					continue;
				}
				AltReferencedMaterial altReferencedMaterial = null;
				foreach (TextureOverride item in _texturePayloadsCache)
				{
					if (material.HasProperty(item.Property))
					{
						if (altReferencedMaterial == null)
						{
							altReferencedMaterial = new AltReferencedMaterial(material);
							_textureLoaders.Add(altReferencedMaterial);
						}
						altReferencedMaterial.SetTexture(item.Property, item.TextureRef.RelativePath);
						if (!string.IsNullOrEmpty(item.Keyword))
						{
							altReferencedMaterial.Material.EnableKeyword(item.Keyword);
						}
						if (!string.IsNullOrEmpty(item.Trigger))
						{
							altReferencedMaterial.Material.SetFloat(item.Trigger, 1f);
						}
					}
				}
				if (altReferencedMaterial != null)
				{
					flag = true;
					materials[i] = altReferencedMaterial.Material;
				}
			}
			if (flag)
			{
				originalMaterial.Key.materials = materials;
			}
		}
	}

	private void OnDestroy()
	{
		FreeLoadedTextures();
	}

	private void FreeLoadedTextures()
	{
		foreach (AltReferencedMaterial textureLoader in _textureLoaders)
		{
			textureLoader.Cleanup();
		}
		_textureLoaders.Clear();
	}
}
