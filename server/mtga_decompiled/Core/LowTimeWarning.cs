using System;
using System.Collections.Generic;
using GreClient.Rules;
using Pooling;
using UnityEngine;
using UnityEngine.Events;
using Wizards.Mtga;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Extensions;

public class LowTimeWarning : MonoBehaviour
{
	public class LowTimeVisibilityChangedEvent : UnityEvent<bool>
	{
	}

	[Serializable]
	private class VariableEasing
	{
		[Header("Approximately the time it will take to reach the target. A smaller value will reach the target faster")]
		[SerializeField]
		public float _smoothDamp = 0.1f;

		private float _velocity;

		public void ResetVelocity()
		{
			_velocity = 0f;
		}

		public float SmoothDamp(float target, float current)
		{
			return Mathf.SmoothDamp(current, target, ref _velocity, _smoothDamp);
		}
	}

	[SerializeField]
	private VariableEasing _easing;

	[SerializeField]
	protected GameObject _rootObj;

	[SerializeField]
	private Animator _animator;

	[SerializeField]
	private TimeoutPip _timeoutPipPrefab;

	[SerializeField]
	protected Transform _timeoutPipRoot;

	[SerializeField]
	private Transform _browserPosition;

	[SerializeField]
	private GameObject _concedeWarningObj;

	protected Vector3 _startPosition;

	private Vector3 _startScale;

	private Quaternion _startRotation;

	private float _timeRunning;

	private MtgTimer _activeTimer;

	private MtgTimer _inactivityTimer;

	private MtgTimer _matchClockTimer;

	private readonly List<TimeoutPip> _timeoutPips = new List<TimeoutPip>();

	private const float FUDGED_TIME_DIFF = 0.5f;

	private const string NOT_VISIBLE_ANIM_NAME = "NotVisible";

	private const string VISIBLE_ANIM_NAME = "Visible";

	public const float SHOW_TIMER_THRESHOLD = 30f;

	public const float HIDE_TIMER_THRESHOLD = 40f;

	private int _visibilityLayerIndex;

	private float _currentPercent;

	private bool _isVisible;

	private bool _isPaused;

	private bool _previouslyVisible;

	private bool _timerHasExpired;

	[HideInInspector]
	public LowTimeVisibilityChangedEvent OnVisibilityChanged = new LowTimeVisibilityChangedEvent();

	private IUnityObjectPool _objectPool;

	private static readonly int Browser = Animator.StringToHash("Browser");

	private static readonly int Pop = Animator.StringToHash("Pop");

	private static readonly int Frozen = Animator.StringToHash("Frozen");

	private static readonly int NoTimeouts = Animator.StringToHash("NoTimeouts");

	private static readonly int BarFill = Animator.StringToHash("BarFill");

	private static readonly int FlareHash = Animator.StringToHash("Flare");

	public bool ActiveandVis => _isVisible;

	public BrowserManager BrowserManager { get; set; }

	protected virtual void Awake()
	{
		_objectPool = Pantry.Get<IUnityObjectPool>();
		_animator.fireEvents = true;
		_visibilityLayerIndex = _animator.GetLayerIndex("Visible");
	}

	private void Start()
	{
		if (_animator.ContainsParameter(Browser) && _browserPosition != null)
		{
			Transform transform = _rootObj.transform;
			_startPosition = transform.position;
			_startRotation = transform.rotation;
			_startScale = transform.localScale;
			BrowserManager.BrowserShown += OnBrowserShown;
			BrowserManager.BrowserHidden += OnBrowserHidden;
		}
	}

	private void OnDestroy()
	{
		OnVisibilityChanged.RemoveAllListeners();
		if (BrowserManager != null)
		{
			BrowserManager.BrowserShown -= OnBrowserShown;
			BrowserManager.BrowserHidden -= OnBrowserHidden;
			BrowserManager = null;
		}
		try
		{
			AudioManager.PostEvent("sfx_ui_timer_stop", base.gameObject);
		}
		catch (Exception)
		{
		}
	}

	public void SetModel(MtgTimer activeTimer, MtgTimer inactiveTimer, MtgTimer matchClockTimer, float timeoutsRemaining)
	{
		_activeTimer = activeTimer;
		_inactivityTimer = inactiveTimer;
		_matchClockTimer = matchClockTimer;
		_timeRunning = calcTimerOffset();
		for (int i = _timeoutPips.Count; (float)i < timeoutsRemaining; i++)
		{
			GameObject gameObject = _objectPool.PopObject(_timeoutPipPrefab.gameObject);
			Transform obj = gameObject.transform;
			obj.SetParent(_timeoutPipRoot);
			obj.ZeroOut();
			_timeoutPips.Add(gameObject.GetComponent<TimeoutPip>());
		}
		float calcTimerOffset()
		{
			MtgTimer mtgTimer = findNonNullTimer();
			if (mtgTimer == null)
			{
				return 0f;
			}
			float num = (float)(DateTime.UtcNow - mtgTimer.CreatedAt).TotalSeconds;
			if (num <= 0f)
			{
				return 0f;
			}
			return num;
		}
		MtgTimer findNonNullTimer()
		{
			if (activeTimer != null)
			{
				return activeTimer;
			}
			if (inactiveTimer != null)
			{
				return inactiveTimer;
			}
			if (matchClockTimer != null)
			{
				return matchClockTimer;
			}
			return null;
		}
	}

	public void OnTimeout(MtgTimer activeTimer)
	{
		_activeTimer = activeTimer;
		if (_inactivityTimer != null)
		{
			_inactivityTimer.ElapsedTime += _timeRunning;
		}
		int count = _timeoutPips.Count;
		if (count > 0)
		{
			TimeoutPip timeoutPip = _timeoutPips[count - 1];
			_timeoutPips.Remove(timeoutPip);
			timeoutPip.TimeoutUse();
		}
		_timeRunning = 0f;
	}

	private void OnBrowserShown(BrowserBase browser)
	{
		_rootObj.transform.SetPositionAndRotation(_browserPosition.position, _browserPosition.rotation);
		_rootObj.transform.localScale = _browserPosition.localScale;
		_animator.SetBool(Browser, value: true);
	}

	private void OnBrowserHidden(BrowserBase browser)
	{
		_rootObj.transform.SetPositionAndRotation(_startPosition, _startRotation);
		_rootObj.transform.localScale = _startScale;
		_animator.SetBool(Browser, value: false);
	}

	private void LateUpdate()
	{
		if (_activeTimer != null)
		{
			bool isPaused = _isPaused;
			_isPaused = _activeTimer.IsPaused;
			bool isVisible = _isVisible;
			float num = _activeTimer.RemainingTime - _timeRunning;
			_isVisible = TimerIsVisible(num);
			if (num <= 0.5f && _timeoutPips.Count == 0 && _activeTimer.Running)
			{
				if (!_timerHasExpired)
				{
					AudioManager.PostEvent("sfx_ui_timer_timeout", base.gameObject);
					AudioManager.PostEvent("sfx_ui_timer_stop", base.gameObject);
					if (_animator.ContainsParameter(Pop))
					{
						_animator.SetTrigger(Pop);
					}
					_timerHasExpired = true;
				}
			}
			else
			{
				bool flag = !isVisible && _isVisible;
				bool flag2 = isVisible && !_isVisible;
				if (flag)
				{
					if (_previouslyVisible)
					{
						_currentPercent = 1f - Mathf.Clamp01((float)_activeTimer.WarningThreshold / 40f);
					}
					else
					{
						_currentPercent = 1f;
						_animator.SetTrigger(FlareHash);
					}
					AudioManager.PostEvent("sfx_ui_timer_appear", base.gameObject);
					_animator.Play("Visible", _visibilityLayerIndex, (!_previouslyVisible) ? 1 : 0);
					_previouslyVisible = true;
					OnVisibilityChanged?.Invoke(arg0: true);
				}
				else if (flag2)
				{
					AudioManager.PostEvent("sfx_ui_timer_stop", base.gameObject);
					_animator.Play("NotVisible", _visibilityLayerIndex, 0f);
					OnVisibilityChanged?.Invoke(arg0: false);
				}
				if (_isVisible)
				{
					if (_animator.ContainsParameter(Frozen))
					{
						_animator.SetBool(Frozen, _isPaused);
					}
					if (isPaused != _isPaused)
					{
						AudioManager.PostEvent(_isPaused ? "sfx_ui_timer_pause" : "sfx_ui_timer_resume", base.gameObject);
					}
					_animator.SetBool(NoTimeouts, _timeoutPips.Count == 0);
					float target = 1f - Mathf.Clamp01(num / 40f);
					_currentPercent = _easing.SmoothDamp(target, _currentPercent);
					_animator.SetFloat(BarFill, _currentPercent);
				}
			}
		}
		else
		{
			_easing.ResetVelocity();
			if (_isVisible)
			{
				AudioManager.PostEvent("sfx_ui_timer_stop", base.gameObject);
				OnVisibilityChanged?.Invoke(arg0: false);
			}
			_isVisible = false;
			_previouslyVisible = false;
			_timerHasExpired = false;
			_animator.Play("NotVisible", _visibilityLayerIndex, 1f);
		}
		if ((_activeTimer != null && _activeTimer.Running) || (_inactivityTimer != null && _inactivityTimer.Running) || (_matchClockTimer != null && _matchClockTimer.Running))
		{
			_timeRunning += Time.deltaTime;
		}
		if ((bool)_concedeWarningObj)
		{
			_concedeWarningObj.UpdateActive(ShouldShowWarning());
		}
		bool ShouldShowWarning()
		{
			if (_inactivityTimer == null || _activeTimer == null)
			{
				return false;
			}
			if (!_inactivityTimer.Running && !_activeTimer.Running)
			{
				return false;
			}
			if (_inactivityTimer == _activeTimer && _previouslyVisible)
			{
				return true;
			}
			if (_timeoutPips.Count > 0)
			{
				return false;
			}
			if (!_isVisible)
			{
				return false;
			}
			float num2 = _inactivityTimer.RemainingTime - _timeRunning;
			if (num2 > (float)_inactivityTimer.WarningThreshold)
			{
				return false;
			}
			float num3 = _activeTimer.RemainingTime - (_timeRunning + 0.5f);
			if (num2 > num3)
			{
				return false;
			}
			return true;
		}
	}

	private bool TimerIsVisible(float timeRemaining)
	{
		float warningThreshold = WarningThreshold(_isVisible, 40f, 30f);
		return ShowTimer(timeRemaining, warningThreshold);
	}

	public static float WarningThreshold(bool isVisible, float hideThreshold, float showThreshold)
	{
		if (isVisible)
		{
			return hideThreshold;
		}
		return showThreshold;
	}

	public static bool ShowTimer(float timeRemaining, float warningThreshold)
	{
		return timeRemaining <= warningThreshold;
	}
}
