using UnityEngine;

public class AltReferencedMaterial
{
	private MaterialTextureReferenceLoader _matTexRefLoader;

	private AssetLoader.AssetTracker<Material> _materialTracker = new AssetLoader.AssetTracker<Material>("AltReferenceMaterial");

	public Material Material { get; private set; }

	public AltReferencedMaterial(Material material)
	{
		Material = material;
		_matTexRefLoader = new MaterialTextureReferenceLoader(Material);
	}

	public AltReferencedMaterial(string materialReference)
	{
		if (!string.IsNullOrWhiteSpace(materialReference))
		{
			Material material = _materialTracker.Acquire(materialReference);
			Material = Object.Instantiate(material);
			Material.name = material.name;
		}
		_matTexRefLoader = new MaterialTextureReferenceLoader(Material);
	}

	public MaterialTextureReferenceLoader GetTextureLoader()
	{
		return _matTexRefLoader;
	}

	public void SetTexture(string property, string texturePath)
	{
		_matTexRefLoader.SetTexture(property, texturePath);
	}

	public void Cleanup()
	{
		_materialTracker.Cleanup();
		_matTexRefLoader.Cleanup();
	}
}
