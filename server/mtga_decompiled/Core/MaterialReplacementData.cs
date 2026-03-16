using System;
using System.Collections.Generic;
using System.Linq;
using Core.Shared.Code.Utilities;
using UnityEngine;
using Wotc.Mtga.Cards;

public class MaterialReplacementData
{
	private Action<int> OnMatAssignment;

	public AltReferencedMaterialAndBlock Original { get; private set; }

	public string OriginalTextureName { get; private set; }

	public List<MeshMaterialIndex> Indices { get; private set; }

	public MaterialOverrideType OverrideType { get; private set; }

	public AltReferencedMaterialAndBlock Override { get; private set; }

	public AltReferencedMaterialAndBlock Instance { get; private set; }

	public MaterialReplacementData(AltReferencedMaterialAndBlock original, List<MeshMaterialIndex> indices, MaterialOverrideType overrideType, Action<int> onMatAssignment = null)
	{
		Original = original;
		OverrideType = overrideType;
		if (original.SharedMaterial.HasProperty(ShaderPropertyIds.MainTexPropId))
		{
			OriginalTextureName = original.SharedMaterial.mainTexture?.name ?? string.Empty;
		}
		else
		{
			OriginalTextureName = string.Empty;
		}
		Indices = new List<MeshMaterialIndex>(indices);
		OnMatAssignment = onMatAssignment;
	}

	public void Reset()
	{
		if (Instance?.SharedMaterial != null)
		{
			Action<int> onMatAssignment = OnMatAssignment;
			if (onMatAssignment != null)
			{
				onMatAssignment?.Invoke(Instance.BlockHashCode);
			}
		}
		Instance = null;
		SetMaterialInternal();
	}

	public void UpdateOverride(AltReferencedMaterialAndBlock newOverride)
	{
		if (Override?.SharedMaterial != null)
		{
			Action<int> onMatAssignment = OnMatAssignment;
			if (onMatAssignment != null)
			{
				onMatAssignment?.Invoke(Override.BlockHashCode);
			}
		}
		Override = newOverride;
		SetMaterialInternal();
	}

	public void UpdateInstance(AltReferencedMaterialAndBlock newInstance)
	{
		if (Instance?.SharedMaterial != null)
		{
			Action<int> onMatAssignment = OnMatAssignment;
			if (onMatAssignment != null)
			{
				onMatAssignment?.Invoke(Instance.BlockHashCode);
			}
		}
		Instance = newInstance;
		SetMaterialInternal();
	}

	public MaterialPropertyBlock CopyFirstPropertyBlock()
	{
		return Indices.FirstOrDefault()?.CopyPropertyBlock();
	}

	public void UpdatePropertyBlocks()
	{
		if ((bool)Instance?.SharedMaterial)
		{
			foreach (MeshMaterialIndex index in Indices)
			{
				index.ApplyPropertyBlock(Instance);
			}
			return;
		}
		if ((bool)Override?.SharedMaterial)
		{
			foreach (MeshMaterialIndex index2 in Indices)
			{
				index2.ApplyPropertyBlock(Override);
			}
			return;
		}
		foreach (MeshMaterialIndex index3 in Indices)
		{
			index3.ApplyPropertyBlock(Original);
		}
	}

	private void SetMaterialInternal()
	{
		if ((bool)Instance?.SharedMaterial)
		{
			foreach (MeshMaterialIndex index in Indices)
			{
				index.ApplyMaterialAndPropertyBlock(Instance);
			}
			return;
		}
		if ((bool)Override?.SharedMaterial)
		{
			foreach (MeshMaterialIndex index2 in Indices)
			{
				index2.ApplyMaterialAndPropertyBlock(Override);
			}
			return;
		}
		foreach (MeshMaterialIndex index3 in Indices)
		{
			index3.ApplyMaterialAndPropertyBlock(Original);
		}
	}
}
