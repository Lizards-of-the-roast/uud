using UnityEngine;

public class SpineStencilRandomizer : MonoBehaviour
{
	[SerializeField]
	private Renderer[] meshRenderers;

	public MaterialPropertyBlock matBlock;

	private static readonly int _stencilRefProperty = Shader.PropertyToID("_StencilRef");

	public int _stencilRef;

	private void Awake()
	{
		_stencilRef = Random.Range(1, 255);
		matBlock = new MaterialPropertyBlock();
		matBlock.SetFloat(_stencilRefProperty, _stencilRef);
		Renderer[] array = meshRenderers;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].SetPropertyBlock(matBlock);
		}
	}

	public void AddMeshRenderers(Renderer[] _meshRenderers)
	{
		meshRenderers = _meshRenderers;
	}
}
