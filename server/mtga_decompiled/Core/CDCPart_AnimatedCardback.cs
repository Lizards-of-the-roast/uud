using Core.Shared.Code.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

public class CDCPart_AnimatedCardback : CDCPart, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[SerializeField]
	private Renderer _renderer;

	private MaterialPropertyBlock _materialProps;

	private int _currentAnimationIndex;

	private Collider _collider;

	private bool _disableCollider;

	private Transform _transform;

	private Collider HoverCollider
	{
		get
		{
			if (null == _collider)
			{
				_collider = base.gameObject.GetComponent<Collider>();
			}
			return _collider;
		}
	}

	private void Start()
	{
		_collider = base.gameObject.GetComponent<Collider>();
		_transform = base.transform;
		_materialProps = new MaterialPropertyBlock();
		_renderer.GetComponent<MeshFilter>().mesh.bounds = new Bounds(_renderer.transform.InverseTransformPoint(_renderer.transform.parent.position), Vector3.one * 3f);
	}

	private void Update()
	{
		if (!(CurrentCamera.Value == null))
		{
			Vector3 forward = _transform.forward;
			Vector3 forward2 = CurrentCamera.Value.transform.forward;
			bool flag = forward.x * forward2.x + forward.y * forward2.y + forward.z * forward2.z < 0f;
			if (!_disableCollider && HoverCollider != null)
			{
				HoverCollider.enabled = flag;
			}
			_renderer.enabled = flag;
		}
	}

	private void SetAnimation(int index)
	{
		_materialProps.SetFloat(ShaderPropertyIds.AnimTexIndexPropId, index);
		_renderer.SetPropertyBlock(_materialProps);
	}

	public void DisableBoxCollider()
	{
		_disableCollider = true;
		if (HoverCollider != null)
		{
			HoverCollider.enabled = false;
		}
	}

	public void SetHoverAnimation()
	{
		SetAnimation(1);
	}

	public void ToggleAnimIndex()
	{
		_currentAnimationIndex = ((_currentAnimationIndex == 0) ? 1 : 0);
		SetAnimation(_currentAnimationIndex);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		ToggleAnimIndex();
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		ToggleAnimIndex();
	}

	public override void HandleCleanup()
	{
		base.HandleCleanup();
		_disableCollider = false;
	}
}
