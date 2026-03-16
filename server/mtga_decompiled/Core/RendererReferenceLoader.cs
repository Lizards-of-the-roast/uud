using System.Collections.Generic;
using UnityEngine;

public class RendererReferenceLoader
{
	private Renderer _renderer;

	private readonly Dictionary<int, MaterialPropertyBlockTextureLoader> _matIndexToBlockTexLoader = new Dictionary<int, MaterialPropertyBlockTextureLoader>();

	public RendererReferenceLoader(Renderer meshRenderer)
	{
		_renderer = meshRenderer;
	}

	public MaterialPropertyBlockTextureLoader GetMaterialPropertyBlockTextureLoader(int materialIndex)
	{
		if (!_matIndexToBlockTexLoader.TryGetValue(materialIndex, out var value))
		{
			MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
			_renderer.GetPropertyBlock(materialPropertyBlock, materialIndex);
			value = new MaterialPropertyBlockTextureLoader(materialPropertyBlock);
			_matIndexToBlockTexLoader[materialIndex] = value;
		}
		return value;
	}

	public void SetPropertyBlockTexture(int materialIndex, string property, string texturePath)
	{
		GetMaterialPropertyBlockTextureLoader(materialIndex).TrySetTexture(property, texturePath);
	}

	public void SetAndApplyPropertyBlockTexture(int materialIndex, string property, string texturePath)
	{
		SetPropertyBlockTexture(materialIndex, property, texturePath);
		_renderer?.SetPropertyBlock(_matIndexToBlockTexLoader[materialIndex].MatBlock, materialIndex);
	}

	public void ApplyPropertyBlocks()
	{
		foreach (int key in _matIndexToBlockTexLoader.Keys)
		{
			_renderer?.SetPropertyBlock(_matIndexToBlockTexLoader[key].MatBlock, key);
		}
	}

	public void Cleanup()
	{
		foreach (int key in _matIndexToBlockTexLoader.Keys)
		{
			_renderer?.SetPropertyBlock(null, key);
			_matIndexToBlockTexLoader[key]?.Cleanup();
		}
		_matIndexToBlockTexLoader.Clear();
	}
}
