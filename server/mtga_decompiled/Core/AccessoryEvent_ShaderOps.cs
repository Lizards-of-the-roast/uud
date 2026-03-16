using UnityEngine;

[CreateAssetMenu(fileName = "AE_SM_New", menuName = "ScriptableObject/AccessoryEvents/ShaderOps", order = 1)]
public class AccessoryEvent_ShaderOps : AccessoryEventSO
{
	[SerializeField]
	private int _rendererMatElementIndex;

	[SerializeField]
	private string _fieldName;

	[SerializeField]
	private Texture _texture;

	[SerializeField]
	private Vector4 _vector;

	[SerializeField]
	private Color _color = Color.black;

	[ColorUsage(true, true)]
	[SerializeField]
	private Color _hdrColor = Color.black;

	public void Execute(Renderer renderer)
	{
		Material material = renderer.materials[_rendererMatElementIndex];
		if (_texture != null)
		{
			material.SetTexture(_fieldName, _texture);
		}
		MaterialPropertyBlock materialPropertyBlock = new MaterialPropertyBlock();
		if (_color != Color.black)
		{
			materialPropertyBlock.SetColor(_fieldName, _color);
			renderer.SetPropertyBlock(materialPropertyBlock, _rendererMatElementIndex);
		}
		if (_hdrColor != Color.black)
		{
			materialPropertyBlock.SetColor(_fieldName, _hdrColor);
			renderer.SetPropertyBlock(materialPropertyBlock, _rendererMatElementIndex);
		}
		if (_vector != Vector4.zero)
		{
			material.SetVector(_fieldName, _vector);
		}
	}
}
