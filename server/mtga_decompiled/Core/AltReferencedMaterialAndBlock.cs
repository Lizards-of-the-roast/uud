using UnityEngine;

public class AltReferencedMaterialAndBlock
{
	private MaterialPropertyBlockTextureLoader _matBlockTexLoader;

	public Material SharedMaterial { get; private set; }

	public MaterialPropertyBlock MatBlock => GetMaterialBlockTextureLoader().MatBlock;

	public int BlockHashCode { get; private set; }

	public AltReferencedMaterialAndBlock(Material sharedMaterial, int blockHashCode = 0, MaterialPropertyBlock matblock = null)
	{
		SharedMaterial = sharedMaterial;
		BlockHashCode = blockHashCode;
		if (matblock != null)
		{
			_matBlockTexLoader = new MaterialPropertyBlockTextureLoader(matblock);
		}
	}

	public MaterialPropertyBlockTextureLoader GetMaterialBlockTextureLoader()
	{
		if (_matBlockTexLoader == null)
		{
			MaterialPropertyBlock matBlock = new MaterialPropertyBlock();
			_matBlockTexLoader = new MaterialPropertyBlockTextureLoader(matBlock);
		}
		return _matBlockTexLoader;
	}

	public AltReferencedMaterialAndBlock CopyWithMatBlock(MaterialPropertyBlock matblock)
	{
		return new AltReferencedMaterialAndBlock(SharedMaterial, BlockHashCode, matblock);
	}

	public AltReferencedMaterial CreateAltReferenceMaterialCopy()
	{
		AltReferencedMaterial altReferencedMaterial = new AltReferencedMaterial(Object.Instantiate(SharedMaterial));
		altReferencedMaterial.GetTextureLoader();
		return altReferencedMaterial;
	}

	public void SetFloat(int nameId, float val)
	{
		GetMaterialBlockTextureLoader().SetFloat(nameId, val);
	}

	public void SetMainTexture(string texturePath)
	{
		GetMaterialBlockTextureLoader().SetMainTexture(texturePath);
	}

	public void SetMainTexture(Texture texture)
	{
		GetMaterialBlockTextureLoader().SetMainTexture(texture);
	}

	public Texture GetMainTexture()
	{
		return GetMaterialBlockTextureLoader().GetMainTexture();
	}

	public void SetTexture(string property, string texturePath)
	{
		GetMaterialBlockTextureLoader().TrySetTexture(property, texturePath);
	}

	public void Cleanup()
	{
		_matBlockTexLoader?.Cleanup();
		_matBlockTexLoader = null;
	}

	public bool HasUsedMaterialBlock()
	{
		return _matBlockTexLoader != null;
	}
}
