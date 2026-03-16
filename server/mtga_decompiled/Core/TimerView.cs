using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TimerView : MonoBehaviour
{
	[Serializable]
	public class ModeData
	{
		public RectTransform transform;

		public CanvasGroup canvasGroup;
	}

	[SerializeField]
	private ModeData _normalData;

	[SerializeField]
	private ModeData _alertData;

	[SerializeField]
	private TextMeshProUGUI _labelText;

	[SerializeField]
	private float _alertSeconds = 10f;

	[SerializeField]
	private float _paddingWidth = 54f;

	[SerializeField]
	private float _fadeDuration = 0.25f;

	[SerializeField]
	private Ease _fadeEase = Ease.InOutSine;

	private RectTransform _thisTransform;

	private float _fullWidth;

	private bool _isStarted;

	private float _startingTime;

	private float _remainingTime;

	private float _currentWidth;

	private ModeData _currentData;

	private void Awake()
	{
		_thisTransform = GetComponent<RectTransform>();
		_fullWidth = _thisTransform.sizeDelta.x;
		_currentWidth = _fullWidth;
	}

	private void Start()
	{
		_normalData.canvasGroup.alpha = 1f;
		SetWidth(_normalData.transform, _fullWidth);
		_alertData.canvasGroup.alpha = 0f;
		_labelText.text = string.Empty;
	}

	private void Update()
	{
		if (_isStarted)
		{
			_remainingTime = Mathf.Max(0f, _remainingTime - Time.deltaTime);
			if (_remainingTime <= _alertSeconds && _currentData == _normalData)
			{
				_currentData = _alertData;
				DOTween.Kill(this);
				_normalData.canvasGroup.DOFade(0f, _fadeDuration).SetEase(_fadeEase).SetTarget(this);
				_alertData.canvasGroup.DOFade(1f, _fadeDuration).SetEase(_fadeEase).SetTarget(this);
			}
			_labelText.text = $":{Mathf.FloorToInt(_remainingTime):00}";
			SetWidth(_currentData.transform, _paddingWidth + (_currentWidth - _paddingWidth) * _remainingTime / _startingTime);
			if (_remainingTime <= 0f)
			{
				_isStarted = false;
			}
		}
	}

	public void StartTimer(float seconds, float maxTime, float widthPercent = 1f)
	{
		_isStarted = true;
		_startingTime = maxTime;
		_remainingTime = seconds;
		_currentWidth = _fullWidth * widthPercent;
		_currentData = ((seconds <= _alertSeconds) ? _alertData : _normalData);
		_labelText.text = $"0:{seconds:00}";
		SetWidth(_thisTransform, _currentWidth);
		SetWidth(_currentData.transform, _currentWidth);
		_currentData.canvasGroup.alpha = 1f;
		GetOtherData(_currentData).canvasGroup.alpha = 0f;
	}

	public void StartTimer()
	{
		_isStarted = true;
	}

	public void StopTimer()
	{
		_isStarted = false;
	}

	private ModeData GetOtherData(ModeData original)
	{
		if (original != _alertData)
		{
			return _alertData;
		}
		return _normalData;
	}

	private static void SetWidth(RectTransform rectTransform, float width)
	{
		Vector2 sizeDelta = rectTransform.sizeDelta;
		sizeDelta.x = width;
		rectTransform.sizeDelta = sizeDelta;
	}
}
