using UnityEngine;

public class MaterialTextureReferenceLoader
{
	private AssetTracker _assetTracker = new AssetTracker();

	public Material Material { get; private set; }

	public MaterialTextureReferenceLoader(Material mat)
	{
		Material = mat;
	}

	public void SetMainTexture(string texturePath)
	{
		SetTexture("_MainTex", texturePath);
	}

	public void SetTexture(string property, string texturePath, Texture tex)
	{
		SetTexture(property, texturePath);
		if (Material.GetTexture(property) == null)
		{
			Material.SetTexture(property, tex);
		}
	}

	public void SetFloat(int propertyId, float val)
	{
		Material.SetFloat(propertyId, val);
	}

	public void SetTexture(string property, string texturePath)
	{
		if (_assetTracker.GetLoadedPath(property) != texturePath)
		{
			Material.SetTexture(property, _assetTracker.AcquireAndTrack<Texture>(property, texturePath));
		}
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
		_assetTracker.Cleanup();
	}
}
