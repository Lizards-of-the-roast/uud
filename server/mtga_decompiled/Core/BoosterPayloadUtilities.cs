using System.Collections.Generic;
using System.IO;
using AssetLookupTree;
using AssetLookupTree.Payloads.Booster;
using UnityEngine;
using Wotc.Mtga.Wrapper;

public static class BoosterPayloadUtilities
{
	private class PoolObject
	{
		public AltReferencedMaterial TextureLoader;

		public readonly Material Mat;

		public float DecayTimer;

		public int RefCount;

		public PoolObject(Material mat, AltReferencedMaterial textureLoader)
		{
			TextureLoader = textureLoader;
			Mat = mat;
			DecayTimer = 30f;
			RefCount = 1;
		}

		public void IncrementRefCount()
		{
			DecayTimer = 30f;
			RefCount++;
		}

		public void DecrementRefCount()
		{
			RefCount--;
			if (RefCount < 0)
			{
				Debug.LogWarningFormat("Negative ref count on mat {0}", Mat.name);
			}
		}

		public void Cleanup()
		{
			Object.Destroy(Mat);
			TextureLoader.Cleanup();
		}
	}

	private const float DECAY_TIME = 30f;

	private static readonly Dictionary<string, PoolObject> _cachedMaterials = new Dictionary<string, PoolObject>(250);

	private static readonly List<string> _cachesPendingDelete = new List<string>(10);

	public static void UpdateDecayTimers(float timeStep)
	{
		foreach (KeyValuePair<string, PoolObject> cachedMaterial in _cachedMaterials)
		{
			if (cachedMaterial.Value.RefCount <= 0)
			{
				cachedMaterial.Value.DecayTimer -= timeStep;
				if (cachedMaterial.Value.DecayTimer < 0f)
				{
					_cachesPendingDelete.Add(cachedMaterial.Key);
				}
			}
		}
		if (_cachesPendingDelete.Count <= 0)
		{
			return;
		}
		foreach (string item in _cachesPendingDelete)
		{
			_cachedMaterials[item].Cleanup();
			_cachedMaterials.Remove(item);
		}
		_cachesPendingDelete.Clear();
	}

	public static void ClearUnusedMaterials()
	{
		if (_cachedMaterials.Count > 0)
		{
			foreach (KeyValuePair<string, PoolObject> cachedMaterial in _cachedMaterials)
			{
				if (cachedMaterial.Value.RefCount <= 0)
				{
					_cachesPendingDelete.Add(cachedMaterial.Key);
				}
			}
		}
		if (_cachesPendingDelete.Count <= 0)
		{
			return;
		}
		foreach (string item in _cachesPendingDelete)
		{
			_cachedMaterials[item].Cleanup();
			_cachedMaterials.Remove(item);
		}
		_cachesPendingDelete.Clear();
	}

	public static void FreeBoosterMaterial(Material material)
	{
		string name = material.name;
		if (_cachedMaterials.ContainsKey(name) && _cachedMaterials[name] != null && _cachedMaterials[name].Mat != null)
		{
			_cachedMaterials[name].DecrementRefCount();
		}
	}

	public static Material GetBoosterMaterial(Material originalMaterial, int collationId, bool showLimitedDecal, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		assetLookupSystem.Blackboard.BoosterCollationMapping = (CollationMapping)collationId;
		assetLookupSystem.Blackboard.MaterialName = originalMaterial?.name;
		assetLookupSystem.Blackboard.TextureName = originalMaterial?.mainTexture?.name;
		Logo logo = null;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Logo> loadedTree))
		{
			logo = loadedTree.GetPayload(assetLookupSystem.Blackboard);
		}
		Background background = null;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<Background> loadedTree2))
		{
			background = loadedTree2.GetPayload(assetLookupSystem.Blackboard);
		}
		LimitedDecal limitedDecal = null;
		if (assetLookupSystem.TreeLoader.TryLoadTree(out AssetLookupTree<LimitedDecal> loadedTree3))
		{
			limitedDecal = loadedTree3.GetPayload(assetLookupSystem.Blackboard);
		}
		List<string> list = new List<string>(5) { originalMaterial.GetInstanceID().ToString() };
		string text = null;
		string text2 = null;
		string text3 = null;
		if (logo != null)
		{
			text3 = logo.TextureRef.RelativePath;
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text3);
			list.Add(fileNameWithoutExtension);
		}
		if (background != null)
		{
			text2 = background.TextureRef.RelativePath;
			string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(text2);
			list.Add(fileNameWithoutExtension2);
		}
		if (showLimitedDecal)
		{
			if (limitedDecal != null)
			{
				text = limitedDecal.TextureRef.RelativePath;
				string fileNameWithoutExtension3 = Path.GetFileNameWithoutExtension(text);
				list.Add(fileNameWithoutExtension3);
			}
			list.Add("LIM");
		}
		string text4 = string.Join("_", list);
		if (_cachedMaterials.ContainsKey(text4) && _cachedMaterials[text4] != null && _cachedMaterials[text4].Mat != null)
		{
			PoolObject poolObject = _cachedMaterials[text4];
			poolObject.IncrementRefCount();
			return poolObject.Mat;
		}
		AltReferencedMaterial altReferencedMaterial = new AltReferencedMaterial(Object.Instantiate(originalMaterial));
		MaterialTextureReferenceLoader textureLoader = altReferencedMaterial.GetTextureLoader();
		altReferencedMaterial.Material.name = text4;
		textureLoader.SetTexture("_Decal1", text3);
		textureLoader.SetTexture("_Decal2", text);
		textureLoader.SetTexture("_MainTex", text2);
		if (logo != null)
		{
			altReferencedMaterial.Material.EnableKeyword("_USEDECAL1_ON");
		}
		else
		{
			altReferencedMaterial.Material.DisableKeyword("_USEDECAL1_ON");
		}
		if (limitedDecal != null)
		{
			altReferencedMaterial.Material.EnableKeyword("_USEDECAL2_ON");
		}
		else
		{
			altReferencedMaterial.Material.DisableKeyword("_USEDECAL2_ON");
		}
		_cachedMaterials[text4] = new PoolObject(altReferencedMaterial.Material, altReferencedMaterial);
		return altReferencedMaterial.Material;
	}
}
