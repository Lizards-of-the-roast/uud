using Core.Shared.Code.Utilities;
using UnityEngine;
using UnityEngine.Serialization;

[RequireComponent(typeof(Animator))]
public class AnimatedUVOffset : MonoBehaviour
{
	[SerializeField]
	private GameObject targetMesh;

	[SerializeField]
	private int targetMaterialSlot = 1;

	[SerializeField]
	private bool normalizedExportData;

	private Animator _animator;

	[SerializeField]
	private string animationParameterName1;

	[SerializeField]
	private string animationParameterName2;

	[FormerlySerializedAs("divisions")]
	[Range(1f, 10f)]
	[SerializeField]
	private int uDivisions = 1;

	[Range(1f, 10f)]
	[SerializeField]
	private int vDivisions = 1;

	private float _uUnit;

	private float _vUnit;

	private SkinnedMeshRenderer _skinnedMeshRenderer;

	private MaterialPropertyBlock _propertyBlock;

	private void Start()
	{
		_animator = GetComponent<Animator>();
		_skinnedMeshRenderer = targetMesh.GetComponent<SkinnedMeshRenderer>();
		_uUnit = 1f / (float)uDivisions;
		_vUnit = 1f / (float)vDivisions;
		_propertyBlock = new MaterialPropertyBlock();
	}

	private void LateUpdate()
	{
		Vector4 vector = new Vector4(1f, 1f, _uUnit * Mathf.Round(1f - _animator.GetFloat(animationParameterName1)), _vUnit * Mathf.Round(1f - _animator.GetFloat(animationParameterName2)));
		Vector4 vector2 = new Vector4(1f, 1f, _uUnit * Mathf.Round(_animator.GetFloat(animationParameterName1) * (float)uDivisions), _vUnit * Mathf.Round(_animator.GetFloat(animationParameterName2) * (float)vDivisions));
		Vector4 value = (normalizedExportData ? vector2 : vector);
		_propertyBlock.SetVector(ShaderPropertyIds.TextureScaleOffsetPropId, value);
		_skinnedMeshRenderer.SetPropertyBlock(_propertyBlock, targetMaterialSlot - 1);
	}
}
