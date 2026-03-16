using System.Collections.Generic;
using UnityEngine;

public class MaterialPropertyBlockTextureLoader
{
	private const string MAIN_TEX_PROP = "_MainTex";

	private AssetTracker _assetTracker = new AssetTracker();

	private Dictionary<string, Texture> _textureCache = new Dictionary<string, Texture>();

	public MaterialPropertyBlock MatBlock { get; private set; }

	public MaterialPropertyBlockTextureLoader(MaterialPropertyBlock matBlock)
	{
		MatBlock = matBlock;
	}

	public void SetMainTexture(string texturePath)
	{
		TrySetTexture("_MainTex", texturePath);
	}

	public void SetMainTexture(Texture texture)
	{
		MatBlock.SetTexture("_MainTex", texture);
		if (texture != null)
		{
			_textureCache["_MainTex"] = texture;
		}
		else if (_textureCache.ContainsKey("_MainTex"))
		{
			_textureCache.Remove("_MainTex");
		}
	}

	public Texture GetMainTexture()
	{
		return MatBlock.GetTexture("_MainTex");
	}

	public bool TrySetTexture(string property, string texturePath, Texture fallbackTex)
	{
		if (!TrySetTexture(property, texturePath))
		{
			if (fallbackTex != null)
			{
				MatBlock.SetTexture(property, fallbackTex);
				_textureCache[property] = fallbackTex;
				return true;
			}
			return false;
		}
		return true;
	}

	public bool TrySetTexture(string property, string texturePath)
	{
		if (_assetTracker.GetLoadedPath(property) == texturePath)
		{
			return true;
		}
		Texture texture = _assetTracker.AcquireAndTrack<Texture>(property, texturePath);
		if (texture == null)
		{
			return false;
		}
		MatBlock.SetTexture(property, texture);
		_textureCache[property] = texture;
		return true;
	}

	public void SetFloat(int nameId, float value)
	{
		MatBlock.SetFloat(nameId, value);
	}

	public void Cleanup()
	{
		Cleanup_Internal(destructed: false);
	}

	public void Destruct()
	{
		Cleanup_Internal(destructed: true);
	}

	private void Cleanup_Internal(bool destructed)
	{
		_textureCache.Clear();
		_assetTracker.Cleanup();
		MatBlock.Clear();
	}
}
