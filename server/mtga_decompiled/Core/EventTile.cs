using System;
using System.Collections.Generic;
using Assets.Core.Shared.Code;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wizards.MDN;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

public class EventTile : MonoBehaviour
{
	public GameObject ParameterContainer;

	public GameObject Attract;

	public EventTileParameter ParameterTilePrefab;

	public Localize TileText;

	public TextMeshProUGUI TimerText;

	public Image Checkbox;

	public Image Stopwatch;

	public Image Lock;

	public Color PreviewColor;

	public Color LockColor;

	public Color CloseColor;

	public Color CompleteColor;

	public TooltipTrigger Tooltip;

	private List<EventTileParameter> _parameters = new List<EventTileParameter>();

	private EventTimerState? _lastSeenState;

	private TimeSpan _lastTimeLeft;

	private static readonly int UnlockedHash = Animator.StringToHash("Unlocked");

	private static readonly int OnHash = Animator.StringToHash("On");

	private bool _desiredUnlockState = true;

	public EventContext EventContext { get; private set; }

	private void Update()
	{
		EventTimerState timerState = EventContext.PlayerEvent.GetTimerState();
		bool flag = timerState == EventTimerState.Preview || timerState == EventTimerState.Unjoined_LockingSoon || timerState == EventTimerState.Joined_ClosingSoon;
		if (timerState != _lastSeenState)
		{
			_lastSeenState = timerState;
			Image stopwatch = Stopwatch;
			(Color, bool) tuple = timerState switch
			{
				EventTimerState.Joined_ClosingSoon => (LockColor, true), 
				EventTimerState.Unjoined_LockingSoon => (CloseColor, true), 
				_ => (Stopwatch.color, false), 
			};
			bool active;
			(stopwatch.color, active) = tuple;
			Stopwatch.gameObject.UpdateActive(active);
			bool flag2 = timerState == EventTimerState.ClosedAndCompleted;
			Checkbox.gameObject.UpdateActive(flag2);
			Checkbox.color = (flag2 ? CompleteColor : Checkbox.color);
			bool flag3 = timerState == EventTimerState.Preview;
			Lock.gameObject.UpdateActive(flag3);
			Lock.color = (flag3 ? PreviewColor : Lock.color);
			TextMeshProUGUI timerText = TimerText;
			timerText.color = timerState switch
			{
				EventTimerState.Preview => PreviewColor, 
				EventTimerState.Unjoined_LockingSoon => LockColor, 
				EventTimerState.Joined_ClosingSoon => CloseColor, 
				_ => TimerText.color, 
			};
			TimerText.gameObject.UpdateActive(flag);
		}
		if (flag)
		{
			TimeSpan timeSpan = EventContext.PlayerEvent.EventInfo.ClosedTime - ServerGameTime.GameTime;
			if (_lastTimeLeft.Minutes != timeSpan.Minutes)
			{
				_lastTimeLeft = timeSpan;
				TimerText.text = timeSpan.To_HH_MM();
			}
		}
	}

	public void SetEventContext(EventContext eventContext)
	{
		EventContext = eventContext;
		TileText.SetText(EventContext.PlayerEvent?.EventUXInfo.TitleLocKey);
		IPlayerEvent playerEvent = EventContext.PlayerEvent;
		bool flag = playerEvent == null || playerEvent.CourseData.CurrentModule != PlayerEventModule.Join;
		Attract.UpdateActive(flag && EventContext.PlayerEvent?.EventUXInfo?.HasEventPage == true);
	}

	public EventContext GetEventContextByParameters()
	{
		EventContext eventContext = null;
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		foreach (EventTileParameter parameter in _parameters)
		{
			dictionary.Add(parameter.ParameterName, parameter.ParameterValueText.text);
		}
		if (EventContext != null && SameParams(EventContext.PlayerEvent.EventUXInfo.Parameters, dictionary))
		{
			eventContext = EventContext;
		}
		return eventContext ?? EventContext;
	}

	private static bool SameParams(Dictionary<string, string> a, Dictionary<string, string> b)
	{
		if (a.Count != b.Count)
		{
			return false;
		}
		foreach (var (key, text3) in a)
		{
			if (!b.TryGetValue(key, out var value))
			{
				return false;
			}
			if (text3 != value)
			{
				return false;
			}
		}
		return true;
	}

	public void SetUnlocked(bool unlocked)
	{
		_desiredUnlockState = unlocked;
		if (base.isActiveAndEnabled)
		{
			GetComponent<Animator>().SetBool(UnlockedHash, _desiredUnlockState);
		}
	}

	public void OnEnable()
	{
		Animator component = GetComponent<Animator>();
		if (component.GetBool(UnlockedHash) != _desiredUnlockState)
		{
			component.SetBool(UnlockedHash, _desiredUnlockState);
		}
	}

	public void SetSelected(bool selected)
	{
		if (base.isActiveAndEnabled)
		{
			GetComponent<Animator>().SetBool(OnHash, selected);
		}
		foreach (EventTileParameter parameter in _parameters)
		{
			parameter.SetEnabled(selected);
		}
	}

	public void SetToolTip(string tooltipText, float durationBeforeShow = 1f)
	{
		Tooltip.TooltipData.Text = tooltipText;
		Tooltip.TooltipProperties.HoverDurationUntilShow = durationBeforeShow;
	}

	public void OnClick()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_eventblade_event_select, base.gameObject);
	}

	public void OnHover()
	{
		AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
	}

	public void RefreshContext()
	{
		string internalEventName = EventContext.PlayerEvent.EventInfo.InternalEventName;
		if (WrapperController.Instance.EventManager.EventsByInternalName.TryGetValue(internalEventName, out var value))
		{
			SetEventContext(value);
		}
	}
}
