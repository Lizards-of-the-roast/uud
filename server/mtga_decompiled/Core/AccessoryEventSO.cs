using System;
using UnityEngine;

public class AccessoryEventSO : ScriptableObject
{
	[Serializable]
	public class cyclerObj
	{
		public int cycleIndex;

		public UnityEngine.Object obj;
	}

	public virtual void Execute()
	{
	}

	private void SetTransform()
	{
	}

	private void DeactivateGameObjects()
	{
	}

	private void ActivateGameObjects()
	{
	}

	private void SetTextureInMaterial(Material mat, string matPropertyName, Texture tex)
	{
		mat.SetTexture(matPropertyName, tex);
	}

	private void SetColorInMaterial(Material mat, string matPropertyName, Color col)
	{
		mat.SetColor(matPropertyName, col);
	}

	private void SwapMaterial(Renderer rend, int materialSlotIndex, Material mat)
	{
		rend.materials[materialSlotIndex] = mat;
	}
}
