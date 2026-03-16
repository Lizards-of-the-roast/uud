using System;
using System.Collections.Generic;
using System.Linq;
using Core.MainNavigation.RewardTrack;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

[ExecuteInEditMode]
[SelectionBase]
public class EPP_OrbSlotView : MonoBehaviour
{
	public OrbSlot OrbSlotModel;

	public int MyIDForServerPairing;

	public MTGALocalizedString Title;

	public MTGALocalizedString Description;

	[EnumFlags]
	public CardColorFlags Color;

	[SerializeField]
	private float _radius;

	[SerializeField]
	[FormerlySerializedAs("_handheldSize")]
	[Tooltip("Used for handheld 16x9")]
	private Vector3 _largerSize = Vector3.one;

	[SerializeField]
	private OrbSlot.OrbState _orbState;

	[SerializeField]
	private LineAnchor _connectorPrefab;

	[SerializeField]
	private EPP_OrbSlotView[] _previousSlots;

	[SerializeField]
	[HideInInspector]
	private RewardTreeView _rewardTree;

	[SerializeField]
	private List<LineAnchor> _connectors;

	[Header("Assets")]
	[SerializeField]
	private GameObject AvailableGlow;

	[SerializeField]
	private GameObject UnlockedGlow;

	[SerializeField]
	private GameObject EdgesWhenUnlocked;

	[SerializeField]
	private Button Button;

	private Animator _animator;

	private EventTrigger _trigger;

	private RewardTreeView _treeView;

	public Action<EPP_OrbSlotView> PointerEnterOrbSlot;

	public Action<EPP_OrbSlotView> ClickOrbSlot;

	public Action<EPP_OrbSlotView> PointerExitOrbSlot;

	public UnityEvent _onSlotPreviouslyUnlocked;

	public UnityEvent _onSlotUnlocked;

	private bool _isSelected;

	private static readonly int Glow = Animator.StringToHash("Glow");

	private static readonly int Filled = Animator.StringToHash("Filled");

	private static readonly int Highlight = Animator.StringToHash("Highlight");

	private static readonly int Selected = Animator.StringToHash("Selected");

	private static readonly int Presenting = Animator.StringToHash("Presenting");

	public bool IsSelected => _isSelected;

	public OrbSlot.OrbState VisibleSlotState => _orbState;

	public void Awake()
	{
		_animator = GetComponent<Animator>();
		if (Button != null)
		{
			Button.gameObject.UpdateActive(active: true);
			_trigger = Button.gameObject.AddComponent(typeof(EventTrigger)) as EventTrigger;
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.PointerEnter;
			entry.callback.AddListener(delegate
			{
				PointerEnterOrbSlot?.Invoke(this);
			});
			_trigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.PointerClick;
			entry2.callback.AddListener(delegate
			{
				ClickOrbSlot?.Invoke(this);
			});
			_trigger.triggers.Add(entry2);
			EventTrigger.Entry entry3 = new EventTrigger.Entry();
			entry3.eventID = EventTriggerType.PointerExit;
			entry3.callback.AddListener(delegate
			{
				PointerExitOrbSlot?.Invoke(this);
			});
			_trigger.triggers.Add(entry3);
		}
	}

	public void Init(OrbSlot orbslot, RewardTreeView controller, bool useLargerSize)
	{
		_treeView = controller;
		OrbSlotModel = orbslot;
		if (useLargerSize)
		{
			base.gameObject.transform.localScale = _largerSize;
		}
	}

	private void OnEnable()
	{
		if (_orbState == OrbSlot.OrbState.Unlocked)
		{
			_onSlotPreviouslyUnlocked.Invoke();
		}
		SetState(_orbState);
	}

	public void SetToModel()
	{
		OrbSlot.OrbState state = OrbSlotModel?.currentState ?? OrbSlot.OrbState.Unavailable;
		if (_previousSlots.Length != 0 && _previousSlots.All((EPP_OrbSlotView slot) => slot.IsSelected))
		{
			state = OrbSlot.OrbState.Available;
		}
		if (_orbState == OrbSlot.OrbState.Available && !string.IsNullOrEmpty(OrbSlotModel?.serverRewardNode.unlockQuestMetric))
		{
			state = OrbSlot.OrbState.Unavailable;
		}
		SetState(state);
	}

	public void SetSelected(bool isSelected)
	{
		_isSelected = isSelected;
		_animator.SetBool(Selected, isSelected);
	}

	public void SetPresenting(bool isPresenting)
	{
		_animator.SetBool(Presenting, isPresenting);
	}

	public void SetState(OrbSlot.OrbState value)
	{
		if (_orbState != OrbSlot.OrbState.Unlocked && value == OrbSlot.OrbState.Unlocked)
		{
			_onSlotUnlocked.Invoke();
		}
		_orbState = value;
		if (!base.isActiveAndEnabled)
		{
			return;
		}
		if (_animator != null)
		{
			_animator.SetBool(Glow, _orbState != OrbSlot.OrbState.Unavailable);
			_animator.SetBool(Filled, _orbState == OrbSlot.OrbState.Unlocked);
		}
		if (AvailableGlow != null)
		{
			AvailableGlow.SetActive(_orbState != OrbSlot.OrbState.Unavailable);
			UnlockedGlow.SetActive(_orbState == OrbSlot.OrbState.Unlocked);
			EdgesWhenUnlocked.SetActive(_orbState == OrbSlot.OrbState.Unlocked);
		}
		if (Button != null)
		{
			Button.enabled = _orbState != OrbSlot.OrbState.Unlocked;
		}
		for (int i = 0; i < _previousSlots.Length; i++)
		{
			if (!(_previousSlots[i] == null))
			{
				_connectors[i].Animator.SetBool(Highlight, _orbState == OrbSlot.OrbState.Available && _previousSlots[i]._orbState == OrbSlot.OrbState.Unlocked);
				_connectors[i].Animator.SetBool(Filled, _orbState == OrbSlot.OrbState.Unlocked);
			}
		}
	}

	public void SetSuggested()
	{
		Button.Select();
	}
}
