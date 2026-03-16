using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Extensions;

public class CDCPart_ControllerAnimatedCardback : CDCPart, IPointerEnterHandler, IEventSystemHandler, IPointerExitHandler
{
	[Header("Elements Disabled Face-Down")]
	[SerializeField]
	private SpriteRenderer _spriteRendererRoot;

	[SerializeField]
	private SpriteRenderer[] _extraSpriteRenderers;

	[SerializeField]
	private MeshRenderer[] _meshRenderers;

	[Header("Elements Enabled/Disabled on Hover")]
	[SerializeField]
	private GameObject[] _objectsToEnable;

	[SerializeField]
	private GameObject[] _objectsToDisable;

	[SerializeField]
	private Animator _targetAnimator;

	private static readonly int Hovering = Animator.StringToHash("Hovering");

	private Collider _collider;

	private Transform _transform;

	private List<Renderer> _renderers;

	public bool DebugMode;

	public void ResetCardback()
	{
		SetActiveState(state: false);
	}

	private void Start()
	{
		if (Application.isPlaying)
		{
			_collider = base.gameObject.GetComponent<Collider>();
			ResetCardback();
			_transform = base.transform;
			_renderers = new List<Renderer>();
			if (_spriteRendererRoot != null)
			{
				_renderers = new List<Renderer>(_spriteRendererRoot.GetComponentsInChildren<Renderer>());
			}
			SpriteRenderer[] extraSpriteRenderers = _extraSpriteRenderers;
			foreach (Renderer item in extraSpriteRenderers)
			{
				_renderers.Add(item);
			}
			MeshRenderer[] meshRenderers = _meshRenderers;
			foreach (Renderer item2 in meshRenderers)
			{
				_renderers.Add(item2);
			}
		}
	}

	private void Update()
	{
		if (!Application.isPlaying || CurrentCamera.Value == null || _transform == null)
		{
			return;
		}
		Vector3 forward = _transform.forward;
		Vector3 forward2 = CurrentCamera.Value.transform.forward;
		bool flag = forward.x * forward2.x + forward.y * forward2.y + forward.z * forward2.z < 0f;
		if (_collider != null && !_cachedViewMetadata.IsMeta)
		{
			_collider.enabled = flag;
		}
		if (_spriteRendererRoot != null)
		{
			_spriteRendererRoot.enabled = flag;
		}
		if (_renderers != null)
		{
			foreach (Renderer renderer in _renderers)
			{
				if (renderer != null)
				{
					renderer.enabled = flag;
				}
			}
		}
		if (_targetAnimator != null)
		{
			_targetAnimator.enabled = flag;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!_cachedViewMetadata.IsMeta)
		{
			SetActiveState(state: true);
			if (_targetAnimator.ContainsParameter(Hovering))
			{
				_targetAnimator.SetBool(Hovering, value: true);
			}
		}
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (!_cachedViewMetadata.IsMeta && _targetAnimator.ContainsParameter(Hovering))
		{
			_targetAnimator.SetBool(Hovering, value: false);
		}
	}

	public void PlayAnimations()
	{
		SetActiveState(state: true);
		if (_targetAnimator.ContainsParameter(Hovering))
		{
			_targetAnimator.SetBool(Hovering, value: true);
		}
	}

	private void SetActiveState(bool state)
	{
		GameObject[] objectsToEnable = _objectsToEnable;
		foreach (GameObject gameObject in objectsToEnable)
		{
			if (gameObject != null)
			{
				gameObject.SetActive(state);
			}
		}
		objectsToEnable = _objectsToDisable;
		foreach (GameObject gameObject2 in objectsToEnable)
		{
			if (gameObject2 != null)
			{
				gameObject2.SetActive(!state);
			}
		}
	}

	public void OnDisable()
	{
		AudioManager.StopSFX(_targetAnimator.gameObject);
	}

	public void AddMeshRenderer(MeshRenderer _MeshRenderertoAdd)
	{
		_meshRenderers = new MeshRenderer[1] { _MeshRenderertoAdd };
	}

	public void AddTargetAnimator(Animator animator)
	{
		_targetAnimator = animator;
	}

	public void AddObjectsToDisable(GameObject[] gameObjects)
	{
		_objectsToDisable = gameObjects;
	}

	public void AddObjectsToEnable(GameObject[] gameObjects)
	{
		_objectsToEnable = gameObjects;
	}

	public void AddExtraSpriteRenderers(SpriteRenderer[] spriteRenderers)
	{
		_extraSpriteRenderers = spriteRenderers;
	}

	public void AddMeshRenderers(MeshRenderer[] meshRenderers)
	{
		_meshRenderers = meshRenderers;
	}
}
