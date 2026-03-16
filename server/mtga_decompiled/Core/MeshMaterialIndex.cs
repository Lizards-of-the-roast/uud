using UnityEngine;

public sealed class MeshMaterialIndex
{
	public Renderer Renderer;

	public int Index;

	public Material GetMaterial()
	{
		if (!Renderer)
		{
			return null;
		}
		Material[] sharedMaterials = Renderer.sharedMaterials;
		if (sharedMaterials.Length <= Index)
		{
			return null;
		}
		return sharedMaterials[Index];
	}

	public MaterialPropertyBlock CopyPropertyBlock()
	{
		if (!Renderer)
		{
			return null;
		}
		if (Renderer.sharedMaterials.Length <= Index)
		{
			return null;
		}
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		Renderer.GetPropertyBlock(materialPropertyBlock);
		return materialPropertyBlock;
	}

	public void ApplyMaterialAndPropertyBlock(AltReferencedMaterialAndBlock refMatBlock)
	{
		if (!Renderer)
		{
			return;
		}
		Material[] sharedMaterials = Renderer.sharedMaterials;
		if (sharedMaterials.Length > Index)
		{
			sharedMaterials[Index] = refMatBlock.SharedMaterial;
			if (refMatBlock.HasUsedMaterialBlock())
			{
				Renderer.SetPropertyBlock(refMatBlock.MatBlock, Index);
			}
			else
			{
				Renderer.SetPropertyBlock(null, Index);
			}
			Renderer.sharedMaterials = sharedMaterials;
		}
	}

	public void ApplyPropertyBlock(AltReferencedMaterialAndBlock refMatBlock)
	{
		if ((bool)Renderer && Renderer.sharedMaterials.Length > Index)
		{
			if (refMatBlock.HasUsedMaterialBlock())
			{
				Renderer.SetPropertyBlock(refMatBlock.MatBlock, Index);
			}
			else
			{
				Renderer.SetPropertyBlock(null, Index);
			}
		}
	}

	public void SetMaterial(Material mat)
	{
		if ((bool)Renderer)
		{
			Material[] sharedMaterials = Renderer.sharedMaterials;
			if (sharedMaterials.Length > Index)
			{
				sharedMaterials[Index] = mat;
				Renderer.sharedMaterials = sharedMaterials;
			}
		}
	}
}
