using System.Collections.Generic;
using Core.Shared.Code.Utilities;
using UnityEngine;

public class CDCPart_Shadow : CDCPart
{
	private const float SHADOW_FADE_SPEED = 5f;

	private const float MAX_ALPHA = 0.5f;

	private static readonly Dictionary<Material, Material> _SharedMaterials = new Dictionary<Material, Material>();

	[SerializeField]
	private Vector3 _lightDirection = Vector3.down;

	private Material _originalMaterial;

	private Projector _projector;

	private bool _isEnabled = true;

	private float _alpha;

	private float _originalNearClip = 10f;

	private float _originalFarClip = 50f;

	private BASE_CDC _cachedParentCDC;

	private Material GetSharedMaterial
	{
		get
		{
			if (!_SharedMaterials.ContainsKey(_originalMaterial))
			{
				_SharedMaterials[_originalMaterial] = Object.Instantiate(_originalMaterial);
				Debug.LogWarning("Created new material instance from: " + _originalMaterial.name);
			}
			return _SharedMaterials[_originalMaterial];
		}
	}

	private Material GetInstanceMaterial
	{
		get
		{
			if (_projector.material == GetSharedMaterial)
			{
				_projector.material = Object.Instantiate(GetSharedMaterial);
			}
			return _projector.material;
		}
	}

	private bool IsInstanceMaterial => _projector.material != GetSharedMaterial;

	private void Start()
	{
		_projector = GetComponentInChildren<Projector>();
		_projector.enabled = false;
		_originalNearClip = _projector.nearClipPlane;
		_originalFarClip = _projector.farClipPlane;
		_originalMaterial = _projector.material;
		_projector.material = GetSharedMaterial;
	}

	private void LateUpdate()
	{
		bool flag = !_cachedDestroyed && _isEnabled && base.transform.position.y > 0f;
		if (Mathf.Approximately(_alpha, 0f) && !flag)
		{
			_projector.enabled = false;
			return;
		}
		if (_cachedParentCDC != null)
		{
			float num = _cachedParentCDC.Root.localEulerAngles.z;
			if (num > 180f)
			{
				num -= 360f;
			}
			_lightDirection.x = Mathf.LerpUnclamped(0f, 0.015f, num / 8f);
		}
		else
		{
			_lightDirection.x = 0f;
		}
		float y = base.transform.lossyScale.y;
		_projector.nearClipPlane = _originalNearClip * y;
		_projector.farClipPlane = _originalFarClip * y;
		_projector.enabled = true;
		base.transform.LookAt(base.transform.position + _lightDirection);
		if (Mathf.Approximately(_alpha, 0.5f) && flag)
		{
			if (IsInstanceMaterial)
			{
				_projector.material = GetSharedMaterial;
			}
		}
		else
		{
			_alpha = Mathf.Clamp(_alpha + Time.deltaTime * 5f * (float)(flag ? 1 : (-1)), 0f, 0.5f);
			GetInstanceMaterial.SetFloat(ShaderPropertyIds.AlphaPropId, _alpha);
		}
	}

	protected override void HandleUpdateInternal()
	{
		base.HandleUpdateInternal();
		if (_cachedCardHolderType == CardHolderType.Battlefield && _cachedModel.IsTapped)
		{
			if (_cachedParentCDC == null || !_cachedParentCDC.ActiveParts.ContainsValue(this))
			{
				_cachedParentCDC = GetComponentInParent<BASE_CDC>();
			}
		}
		else
		{
			_cachedParentCDC = null;
		}
	}

	public void SetShadows(bool shouldBeEnabled)
	{
		_isEnabled = shouldBeEnabled;
	}
}
