using UnityEngine;

public class SpineUtilPassRendererToParticleSystem : MonoBehaviour
{
	private MeshFilter _meshFilter;

	private Mesh _meshInstance;

	[SerializeField]
	private ParticleSystemRenderer[] particleSystemRenderers;

	private void Awake()
	{
		if (_meshFilter == null)
		{
			if (!TryGetComponent<MeshFilter>(out _meshFilter))
			{
				return;
			}
			_meshInstance = _meshFilter.mesh;
		}
		ParticleSystemRenderer[] array = particleSystemRenderers;
		foreach (ParticleSystemRenderer particleSystemRenderer in array)
		{
			if (particleSystemRenderer != null)
			{
				particleSystemRenderer.mesh = _meshInstance;
			}
		}
	}
}
