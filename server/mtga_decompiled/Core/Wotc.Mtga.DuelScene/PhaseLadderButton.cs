using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public abstract class PhaseLadderButton : MonoBehaviour, IPointerEnterHandler, IEventSystemHandler
{
	[Header("Phases")]
	public Phase Phase;

	public Step Step;

	public GREPlayerNum StopOnlyThisPlayer;

	[Header("Tooltips")]
	public MTGALocalizedString PlayerStopTooltip;

	public MTGALocalizedString PlayerStopNextTooltip;

	public MTGALocalizedString OpponentStopTooltip;

	public MTGALocalizedString OpponentStopNextTooltip;

	public MTGALocalizedString PhaseTooltip;

	[Header("Stop Types")]
	[SerializeField]
	private List<StopType> _playerStopTypes;

	[SerializeField]
	private List<StopType> _opponentStopTypes;

	[Header("Assets")]
	[SerializeField]
	private EventTrigger _stop;

	private ClickAndHoldButton _buttonToggleFullControl;

	private FullControlToggle _fullControlToggle;

	protected Animator _animator;

	private bool _lit;

	private bool _highlight;

	private ButtonPhaseLadder _phaseLadder;

	public bool AllowStop
	{
		get
		{
			if (_stop != null)
			{
				return _stop.enabled;
			}
			return false;
		}
		set
		{
			if (_stop != null)
			{
				_stop.enabled = value;
			}
		}
	}

	public bool Lit
	{
		get
		{
			return _lit;
		}
		set
		{
			_lit = value;
			SetActive(_lit);
		}
	}

	public bool Highlight
	{
		get
		{
			return _highlight;
		}
		set
		{
			_highlight = value;
			SetHighlight(_highlight);
		}
	}

	public GREPlayerNum ActivePlayer { get; private set; }

	public SettingScope AppliesTo { get; private set; }

	public SettingStatus StopState { get; private set; }

	public IReadOnlyList<StopType> GetStopTypesForPlayer(GREPlayerNum player)
	{
		return player switch
		{
			GREPlayerNum.LocalPlayer => _playerStopTypes, 
			GREPlayerNum.Opponent => _opponentStopTypes, 
			_ => Array.Empty<StopType>(), 
		};
	}

	public virtual void SetActivePlayer(GREPlayerNum activePlayer)
	{
		ActivePlayer = activePlayer;
	}

	public void Init(ButtonPhaseLadder phaseLadder)
	{
		_phaseLadder = phaseLadder;
	}

	public void InitFullControlToggle(FullControl fullControl)
	{
		if (!(_buttonToggleFullControl != null) && !(_stop == null) && fullControl is FullControlToggle fullControlToggle)
		{
			_fullControlToggle = fullControlToggle;
			_buttonToggleFullControl = _stop.gameObject.AddComponent<ClickAndHoldButton>();
			_buttonToggleFullControl.ClickAndHold += OnToggleFullControl;
		}
	}

	protected virtual void OnEnable()
	{
		_animator = GetComponent<Animator>();
		SetActive(Lit);
		SetHighlight(Highlight);
		SetActivePlayer(ActivePlayer);
		SetStop(AppliesTo, StopState);
	}

	protected abstract void SetActive(bool active);

	protected abstract void SetHighlight(bool highlight);

	public virtual void SetStop(SettingScope stopScope, SettingStatus stopState)
	{
		AppliesTo = stopScope;
		StopState = stopState;
	}

	protected virtual void OnDestroy()
	{
		if (_buttonToggleFullControl != null)
		{
			_buttonToggleFullControl.ClickAndHold -= OnToggleFullControl;
		}
	}

	public void ToggleStop()
	{
		if (!(_phaseLadder == null) && AllowStop)
		{
			_phaseLadder.ToggleTransientStop(this);
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!(_phaseLadder == null))
		{
			_phaseLadder.DisplayTooltipForPhaseIcon(this);
		}
	}

	private void OnToggleFullControl()
	{
		if (!(_fullControlToggle == null))
		{
			if (_fullControlToggle.Visible)
			{
				_fullControlToggle.HideToggle();
			}
			else
			{
				_fullControlToggle.ShowToggle();
			}
		}
	}
}
