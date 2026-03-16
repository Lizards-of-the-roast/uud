using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Wotc.Mtga.Wrapper.Draft;

public class DraftTimer : MonoBehaviour
{
	[SerializeField]
	private Slider _draftTimerSlider;

	[SerializeField]
	private Image _draftTimerImage;

	[SerializeField]
	private TextMeshProUGUI _draftTimerText;

	private TimeSpan _currentTime;

	public void UpdateTime(float remainingSeconds, float totalSeconds)
	{
		TimeSpan timeSpan = TimeSpan.FromSeconds(Mathf.CeilToInt(remainingSeconds));
		if (_currentTime != timeSpan)
		{
			_currentTime = timeSpan;
			_draftTimerText.text = $"{_currentTime:m':'ss}";
		}
		if ((bool)_draftTimerSlider)
		{
			_draftTimerSlider.value = remainingSeconds / totalSeconds;
		}
		if ((bool)_draftTimerImage)
		{
			_draftTimerImage.fillAmount = remainingSeconds / totalSeconds;
		}
	}
}
