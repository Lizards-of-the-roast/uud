using UnityEngine;

public class StencilRefRandomizer : MonoBehaviour
{
	[SerializeField]
	private MeshRenderer[] meshRenderers;

	[SerializeField]
	private SpriteRenderer[] spriteRenderers;

	[SerializeField]
	private ParticleSystemRenderer[] particleSystemRenderers;

	private static readonly int _stencilRefProperty = Shader.PropertyToID("_StencilRef");

	private int _stencilRef;

	private void Awake()
	{
		_stencilRef = Random.Range(1, 255);
		MeshRenderer[] array = meshRenderers;
		foreach (MeshRenderer stencilValue in array)
		{
			SetStencilValue(stencilValue);
		}
		SpriteRenderer[] array2 = spriteRenderers;
		foreach (SpriteRenderer stencilValue2 in array2)
		{
			SetStencilValue(stencilValue2);
		}
		ParticleSystemRenderer[] array3 = particleSystemRenderers;
		foreach (ParticleSystemRenderer stencilValue3 in array3)
		{
			SetStencilValue(stencilValue3);
		}
	}

	private void SetStencilValue(MeshRenderer meshRenderer)
	{
		if (!(meshRenderer != null))
		{
			return;
		}
		Material[] materials = meshRenderer.materials;
		foreach (Material material in materials)
		{
			if (material.HasProperty(_stencilRefProperty))
			{
				material.SetInt(_stencilRefProperty, _stencilRef);
			}
		}
	}

	private void SetStencilValue(SpriteRenderer spriteRenderer)
	{
		if (!(spriteRenderer != null))
		{
			return;
		}
		Material[] materials = spriteRenderer.materials;
		foreach (Material material in materials)
		{
			if (material.HasProperty(_stencilRefProperty))
			{
				material.SetInt(_stencilRefProperty, _stencilRef);
			}
		}
	}

	private void SetStencilValue(ParticleSystemRenderer particleSystemRenderer)
	{
		Material[] materials = particleSystemRenderer.materials;
		foreach (Material material in materials)
		{
			if (material.HasProperty(_stencilRefProperty))
			{
				material.SetInt(_stencilRefProperty, _stencilRef);
			}
		}
	}
}
