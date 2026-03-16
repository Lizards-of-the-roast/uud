using System;
using System.Collections.Generic;
using Assets.Core.Shared.Code;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class TimerComponent : EventComponent
{
	[SerializeField]
	private GameObject _timerHeader;

	[SerializeField]
	private TextMeshProUGUI _timerHeaderText;

	[SerializeField]
	private Image _stopwatchImage;

	[SerializeField]
	private Image _lockImage;

	[SerializeField]
	private GameObject _buttonShapedTimer;

	[SerializeField]
	private TextMeshProUGUI _buttonShapedTimerText;

	[SerializeField]
	private Color _inPreviewColor;

	[SerializeField]
	private Color _lockColor;

	[SerializeField]
	private Color _endingSoonColor;

	public Action OnTimerEnded;

	private bool _active;

	private DateTime _expiredTime;

	private MTGALocalizedString _text;

	public void Preview(DateTime time, MTGALocalizedString text)
	{
		_active = true;
		text.Parameters = new Dictionary<string, string>(1);
		_text = text;
		_expiredTime = time;
		_timerHeader.UpdateActive(active: true);
		_timerHeaderText.gameObject.UpdateActive(active: true);
		_timerHeaderText.color = _inPreviewColor;
		_timerHeaderText.text = text;
		_lockImage.gameObject.UpdateActive(active: true);
		_lockImage.color = _inPreviewColor;
		_buttonShapedTimer.UpdateActive(active: true);
		_stopwatchImage.gameObject.UpdateActive(active: false);
	}

	public void Closed(MTGALocalizedString text)
	{
		_active = false;
		_timerHeader.UpdateActive(active: true);
		_timerHeaderText.gameObject.UpdateActive(active: true);
		_timerHeaderText.color = _endingSoonColor;
		_timerHeaderText.text = text;
		_lockImage.gameObject.UpdateActive(active: true);
		_lockImage.color = _endingSoonColor;
		_stopwatchImage.gameObject.UpdateActive(active: false);
		_buttonShapedTimer.UpdateActive(active: false);
	}

	public void EndingSoon(DateTime time, MTGALocalizedString text)
	{
		_setTimer(time, _endingSoonColor, text);
	}

	public void LockingSoon(DateTime time, MTGALocalizedString text)
	{
		_setTimer(time, _lockColor, text);
	}

	private void _setTimer(DateTime time, Color color, MTGALocalizedString text)
	{
		_active = true;
		text.Parameters = new Dictionary<string, string>(1);
		_text = text;
		_expiredTime = time;
		_timerHeader.UpdateActive(active: true);
		_timerHeaderText.gameObject.UpdateActive(active: true);
		_timerHeaderText.color = color;
		_stopwatchImage.gameObject.UpdateActive(active: true);
		_stopwatchImage.color = color;
		_lockImage.gameObject.UpdateActive(active: false);
		_buttonShapedTimer.UpdateActive(active: false);
	}

	private void Update()
	{
		if (_active)
		{
			if (ServerGameTime.GameTime >= _expiredTime)
			{
				OnTimerEnded?.Invoke();
				return;
			}
			string text = (_expiredTime - ServerGameTime.GameTime).To_HH_MM_SS();
			_text.Parameters["timeLeft"] = text;
			_timerHeaderText.text = _text;
			_buttonShapedTimerText.text = text;
		}
	}
}
