using System;
using System.Collections.Generic;
using GreClient.Rules;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

[RequireComponent(typeof(Animator))]
public class MatchTimer : MonoBehaviour
{
	private readonly struct TimerThreshold
	{
		public readonly uint LowTime;

		public readonly uint CriticalTime;

		public TimerThreshold(uint lowTime, uint criticalTime)
		{
			LowTime = lowTime;
			CriticalTime = criticalTime;
		}
	}

	private static readonly IReadOnlyDictionary<MatchWinCondition, IReadOnlyDictionary<uint, TimerThreshold>> _timerThresholdsByTypeAndNumber = new Dictionary<MatchWinCondition, IReadOnlyDictionary<uint, TimerThreshold>>
	{
		[MatchWinCondition.SingleElimination] = new Dictionary<uint, TimerThreshold> { [1u] = new TimerThreshold(10u, 6u) },
		[MatchWinCondition.Best2Of3] = new Dictionary<uint, TimerThreshold>
		{
			[1u] = new TimerThreshold(20u, 15u),
			[2u] = new TimerThreshold(15u, 12u),
			[3u] = new TimerThreshold(10u, 9u)
		},
		[MatchWinCondition.Best3Of5] = new Dictionary<uint, TimerThreshold>
		{
			[1u] = new TimerThreshold(40u, 30u),
			[2u] = new TimerThreshold(30u, 20u),
			[3u] = new TimerThreshold(20u, 15u),
			[4u] = new TimerThreshold(15u, 12u),
			[5u] = new TimerThreshold(10u, 9u)
		}
	};

	[SerializeField]
	private TMP_Text _matchTimerText;

	[SerializeField]
	private TMP_Text _criticalTimeText;

	[SerializeField]
	private EventTrigger _hoverArea;

	private Animator _animator;

	private MtgTimer _matchTimer;

	private TimerThreshold _timerThreshold;

	private float _timeRunning;

	private float _warnCountdownTimer;

	private bool _hovering;

	private const int ANIM_STATE_DEFAULT = 0;

	private const int ANIM_STATE_NULL = 1;

	private const int ANIM_STATE_HOVER = 2;

	private const int ANIM_STATE_CRITICAL = 3;

	private const string ANIM_TRIGGER_WARN = "Warn";

	private const int WARN_COUNTDOWN_START = 15;

	private void Awake()
	{
		_animator = GetComponent<Animator>();
		EventTrigger.TriggerEvent triggerEvent = new EventTrigger.TriggerEvent();
		triggerEvent.AddListener(OnIconPointerEnter);
		_hoverArea.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerEnter,
			callback = triggerEvent
		});
		EventTrigger.TriggerEvent triggerEvent2 = new EventTrigger.TriggerEvent();
		triggerEvent2.AddListener(OnIconPointerExit);
		_hoverArea.triggers.Add(new EventTrigger.Entry
		{
			eventID = EventTriggerType.PointerExit,
			callback = triggerEvent2
		});
	}

	private void LateUpdate()
	{
		if (_matchTimer == null)
		{
			return;
		}
		TimeSpan remainingTimeInternal = TimeSpan.FromSeconds(_matchTimer.RemainingTime - _timeRunning);
		int num = calcAnimState(_timerThreshold, remainingTimeInternal, _hovering);
		_animator.SetInteger("State", num);
		_matchTimerText.text = remainingTimeInternal.ToString("mm\\:ss");
		if (num == 3)
		{
			int num2 = remainingTimeInternal.Minutes + 1;
			string localizedText = Languages.ActiveLocProvider.GetLocalizedText("DuelScene/Warning/MatchClockLowTime", ("minutesRemaining", num2.ToString()));
			_criticalTimeText.text = localizedText;
		}
		if (!_matchTimer.Running)
		{
			return;
		}
		float deltaTime = Time.deltaTime;
		_timeRunning += deltaTime;
		if (inWarnTime(_timerThreshold, remainingTimeInternal))
		{
			_warnCountdownTimer -= deltaTime;
			if (_warnCountdownTimer <= 0f)
			{
				_warnCountdownTimer = 15f;
				_animator.SetTrigger("Warn");
			}
		}
		static int calcAnimState(TimerThreshold threshold, TimeSpan remainingTimeInternal2, bool hoveringInternal)
		{
			if (inCriticalTime(threshold, remainingTimeInternal2))
			{
				return 3;
			}
			if (hoveringInternal)
			{
				return 2;
			}
			return 0;
		}
		static bool inCriticalTime(TimerThreshold threshold, TimeSpan timeSpan)
		{
			return timeSpan.TotalMinutes < (double)threshold.CriticalTime;
		}
		static bool inLowTime(TimerThreshold threshold, TimeSpan timeSpan)
		{
			return timeSpan.TotalMinutes < (double)threshold.LowTime;
		}
		static bool inWarnTime(TimerThreshold threshold, TimeSpan remainingTimeInternal2)
		{
			if (!inCriticalTime(threshold, remainingTimeInternal2))
			{
				return inLowTime(threshold, remainingTimeInternal2);
			}
			return false;
		}
	}

	private void OnIconPointerEnter(BaseEventData evtData)
	{
		_hovering = true;
	}

	private void OnIconPointerExit(BaseEventData evtData)
	{
		_hovering = false;
	}

	public void SetGameInfo(GameInfo gameInfo)
	{
		if (_timerThresholdsByTypeAndNumber.TryGetValue(gameInfo.MatchWinCondition, out var value) && value.TryGetValue(gameInfo.GameNumber, out var value2))
		{
			_timerThreshold = value2;
			return;
		}
		Debug.LogError("Unexpected combination of game type and number, no known TimerThreshold configuration!\n" + $"Type: {gameInfo.MatchWinCondition}, Number: {gameInfo.GameNumber}");
		_timerThreshold = new TimerThreshold(20u, 15u);
	}

	public void SetMatchTimer(MtgTimer matchTimer)
	{
		_timeRunning = calcTimerOffset(matchTimer);
		_matchTimer = matchTimer;
		if (_matchTimer == null)
		{
			_animator.SetInteger("State", 1);
		}
		static float calcTimerOffset(MtgTimer matchTimerInternal)
		{
			if (matchTimerInternal == null)
			{
				return 0f;
			}
			float num = (float)(DateTime.UtcNow - matchTimerInternal.CreatedAt).TotalSeconds;
			if (num <= 0f)
			{
				return 0f;
			}
			return num;
		}
	}
}
