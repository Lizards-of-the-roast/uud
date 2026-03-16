using Core.Shared.Code.Utilities;
using UnityEngine;

public sealed class GlowPointLocationSetter : MonoBehaviour
{
	[SerializeField]
	private Renderer _renderer;

	[SerializeField]
	private Transform _glowPointTransform;

	[SerializeField]
	private int _glowMaterialIndex;

	private Material _glowMaterial;

	private void Start()
	{
		if (_renderer == null)
		{
			Debug.LogError("Missing Renderer");
			return;
		}
		if (_glowMaterialIndex > _renderer.materials.Length - 1)
		{
			Debug.LogError("Renderer has no materials");
			return;
		}
		_glowMaterial = _renderer.materials[_glowMaterialIndex];
		if (_glowMaterial == null)
		{
			Debug.LogError("Missing Glow Material");
		}
		else if (!_glowMaterial.HasProperty(ShaderPropertyIds.GlowPositionPropId) || !_glowMaterial.HasProperty(ShaderPropertyIds.GlowScalePropId))
		{
			_glowMaterial = null;
			Debug.LogError("Material in not Glow Material");
		}
	}

	private void Update()
	{
		if (!(_glowMaterial == null))
		{
			_glowMaterial.SetFloat(ShaderPropertyIds.GlowScalePropId, _glowPointTransform.lossyScale.x);
			_glowMaterial.SetVector(ShaderPropertyIds.GlowPositionPropId, _glowPointTransform.position);
		}
	}
}
