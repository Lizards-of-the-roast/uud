using System;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Cards.ArtCrops;

public class MeshRendererReferenceLoader
{
	private MeshRenderer _meshRenderer;

	private readonly Dictionary<int, MaterialPropertyBlockTextureLoader> _matIndexToBlockTexLoader = new Dictionary<int, MaterialPropertyBlockTextureLoader>();

	public MeshRendererReferenceLoader(MeshRenderer meshRenderer)
	{
		_meshRenderer = meshRenderer;
	}

	public MaterialPropertyBlockTextureLoader GetMaterialPropertyBlockTextureLoader(int materialIndex)
	{
		if (!_matIndexToBlockTexLoader.TryGetValue(materialIndex, out var value))
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			_meshRenderer.GetPropertyBlock(materialPropertyBlock, materialIndex);
			value = new MaterialPropertyBlockTextureLoader(materialPropertyBlock);
			_matIndexToBlockTexLoader[materialIndex] = value;
		}
		return value;
	}

	public void SetPropertyBlockTexture(int materialIndex, string property, string texturePath, Texture fallbackTexture = null)
	{
		GetMaterialPropertyBlockTextureLoader(materialIndex).TrySetTexture(property, texturePath, fallbackTexture);
	}

	public void CropWithPropertyBlock(int materialIndex, ArtCrop crop, Texture fallbackTexture = null)
	{
		crop?.SetToPropertyBlock(_matIndexToBlockTexLoader[materialIndex].MatBlock);
	}

	public Material[] GetSharedMaterials()
	{
		if (!(_meshRenderer != null))
		{
			return Array.Empty<Material>();
		}
		return _meshRenderer.sharedMaterials;
	}

	public void ApplyPropertyBlocks()
	{
		foreach (int key in _matIndexToBlockTexLoader.Keys)
		{
			if (_meshRenderer != null)
			{
				_meshRenderer.SetPropertyBlock(_matIndexToBlockTexLoader[key].MatBlock, key);
			}
		}
	}

	public void Cleanup()
	{
		foreach (int key in _matIndexToBlockTexLoader.Keys)
		{
			if (_meshRenderer != null)
			{
				_meshRenderer.SetPropertyBlock(null, key);
			}
			_matIndexToBlockTexLoader[key]?.Cleanup();
		}
		_matIndexToBlockTexLoader.Clear();
	}

	public void Destruct()
	{
		foreach (int key in _matIndexToBlockTexLoader.Keys)
		{
			if (_meshRenderer != null)
			{
				_meshRenderer.SetPropertyBlock(null, key);
			}
			_matIndexToBlockTexLoader[key]?.Destruct();
		}
		_matIndexToBlockTexLoader.Clear();
	}
}
