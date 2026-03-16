using Core.Shared.Code.Utilities;
using UnityEngine;

public class DissolveTester : MonoBehaviour
{
	[Range(0f, 1f)]
	public float DissolveAmount;

	public bool UseDissolved;

	private void Start()
	{
	}

	private void Update()
	{
		Renderer[] componentsInChildren = GetComponentsInChildren<Renderer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			Material[] materials = componentsInChildren[i].materials;
			foreach (Material material in materials)
			{
				if (UseDissolved)
				{
					material.EnableKeyword("_USEDISSOLVED_ON");
					material.SetFloat(ShaderPropertyIds.DissolveAmountPropId, DissolveAmount);
				}
				else
				{
					material.DisableKeyword("_USEDISSOLVED_ON");
					material.SetFloat(ShaderPropertyIds.DissolveAmountPropId, 0f);
				}
			}
		}
	}
}
