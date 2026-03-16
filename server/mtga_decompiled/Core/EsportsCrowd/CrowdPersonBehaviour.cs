using System.Collections.Generic;
using Core.Shared.Code.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;

namespace EsportsCrowd;

public class CrowdPersonBehaviour : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
{
	[SerializeField]
	private CrowdPersonSettings _settings;

	[SerializeField]
	private MeshRenderer _renderer;

	[SerializeField]
	private float _mouseOverHype = 10f;

	private readonly CrowdPersonStatus _status = new CrowdPersonStatus();

	private readonly List<CrowdPersonBehaviour> _neighbors = new List<CrowdPersonBehaviour>();

	private readonly List<HypeEvent> _adjacentHypeEvents = new List<HypeEvent>();

	private MaterialPropertyBlock _materialProps;

	private float _returnToIdleTimer;

	private float _awarenessTimer;

	private bool _dirty;

	public CrowdPersonStatus Status => _status;

	public void SetDirty()
	{
		_dirty = true;
	}

	public void Init()
	{
		_materialProps = new MaterialPropertyBlock();
		SetDirty();
		_settings.InitPerson(this);
		FindNeighbors();
		_materialProps.SetFloat(ShaderPropertyIds.AnimOffsetPropId, Random.Range(0.1f, 0.9f));
		SetAnimation(_settings.IDLE_ANIMATION_INDEX);
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		HandleHypeEvent(new HypeEvent(null, _mouseOverHype));
	}

	public void Update()
	{
		if (_returnToIdleTimer > 0f)
		{
			_returnToIdleTimer -= Time.deltaTime;
			if (_returnToIdleTimer <= 0f)
			{
				SetAnimation(_settings.IDLE_ANIMATION_INDEX);
			}
		}
		if (_awarenessTimer > 0f && _adjacentHypeEvents.Count > 0)
		{
			_awarenessTimer -= Time.deltaTime;
			if (_awarenessTimer <= 0f)
			{
				HypeEvent hypeEvent = _adjacentHypeEvents[0];
				_adjacentHypeEvents.RemoveAt(0);
				HandleHypeEvent(hypeEvent);
				_awarenessTimer += _status.HypeAwarenessSpeed;
			}
		}
		if (_dirty)
		{
			_renderer.SetPropertyBlock(_materialProps);
			_dirty = false;
		}
	}

	public void SetSkinColor(Color color)
	{
		_materialProps.SetColor(ShaderPropertyIds.SkinColorVariantPropId, color);
		SetDirty();
	}

	public void SetShirtColor(Color color)
	{
		_materialProps.SetColor(ShaderPropertyIds.ShirtColorVariantPropId, color);
		SetDirty();
	}

	public void SetSleeveColor(Color color)
	{
		_materialProps.SetColor(ShaderPropertyIds.SleevesColorVariantPropId, color);
		SetDirty();
	}

	public void SetPantsColor(Color color)
	{
		_materialProps.SetColor(ShaderPropertyIds.PantsColorVariantPropId, color);
		SetDirty();
	}

	public void SetFlagColor(Color color)
	{
		_materialProps.SetColor(ShaderPropertyIds.FlagColorVariantPropId, color);
		SetDirty();
	}

	public void SetAnimation(int index)
	{
		_returnToIdleTimer = ((index == _settings.IDLE_ANIMATION_INDEX) ? 0f : _status.HypeRecoverySpeed);
		_materialProps.SetFloat(ShaderPropertyIds.AnimTexIndexPropId, index);
		SetDirty();
	}

	public void HandleHypeEvent(HypeEvent hypeEvent)
	{
		hypeEvent.AffectedPersons.Add(this);
		_status.HandleEvent(ref hypeEvent, out var affinityChanged, out var cheerTriggered);
		if (affinityChanged)
		{
			SetShirtColor(_settings.GetAffinityColor(_status.CurrentAffinity.Value));
			SetFlagColor(_settings.GetAffinityColor(_status.CurrentAffinity.Value));
		}
		if (cheerTriggered)
		{
			SetAnimation(_settings.CHEER_ANIMATION_INDEX);
		}
		if (hypeEvent.Value > _settings.MINIMUM_HYPE_FALLOVER)
		{
			foreach (CrowdPersonBehaviour neighbor in _neighbors)
			{
				if (!hypeEvent.AffectedPersons.Contains(neighbor))
				{
					HypeEvent hypeEvent2 = new HypeEvent(hypeEvent);
					hypeEvent2.Value = hypeEvent.Value / (float)_neighbors.Count;
					HypeEvent hypeEvent3 = hypeEvent2;
					neighbor.HandleAdjacentHypeEvent(hypeEvent3);
				}
			}
		}
		SetDirty();
	}

	private void HandleAdjacentHypeEvent(HypeEvent hypeEvent)
	{
		_adjacentHypeEvents.Add(hypeEvent);
		if (_awarenessTimer <= 0f)
		{
			_awarenessTimer = _status.HypeAwarenessSpeed;
		}
	}

	private void FindNeighbors()
	{
		Collider[] array = new Collider[15];
		int num = Physics.OverlapSphereNonAlloc(base.transform.position + Vector3.up * 0.5f, _status.HypeRange, array);
		for (int i = 0; i < num; i++)
		{
			Collider collider = array[i];
			if (!(collider.gameObject == base.gameObject))
			{
				CrowdPersonBehaviour component = collider.GetComponent<CrowdPersonBehaviour>();
				if (!(component == null))
				{
					_neighbors.Add(component);
				}
			}
		}
	}

	private void OnDrawGizmosSelected()
	{
		if (_status == null || _materialProps == null)
		{
			return;
		}
		Gizmos.color = Color.grey;
		Gizmos.DrawWireSphere(base.transform.position + Vector3.up * 0.5f, _status.HypeRange);
		Gizmos.color = _materialProps.GetColor(ShaderPropertyIds.ShirtColorVariantPropId);
		Gizmos.DrawRay(base.transform.position, Vector3.up);
		foreach (CrowdPersonBehaviour neighbor in _neighbors)
		{
			Gizmos.color = Color.black;
			Gizmos.DrawRay(neighbor.transform.position, Vector3.up);
		}
	}
}
