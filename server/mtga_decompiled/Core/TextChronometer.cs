using System;
using TMPro;
using UnityEngine;

public class TextChronometer : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _timeWaitingText;

	private float _timeWaiting;

	private bool _tickUp;

	public bool IsRunning { get; private set; }

	public void Update()
	{
		if (IsRunning && (_tickUp || _timeWaiting > 0f))
		{
			_timeWaiting += (_tickUp ? Time.deltaTime : (0f - Time.deltaTime));
			TimeSpan timeSpan = TimeSpan.FromSeconds(_timeWaiting);
			string text = $"{timeSpan.Minutes:D1}:{timeSpan.Seconds:D2}";
			_timeWaitingText.text = text;
		}
	}

	public void StartCountDown(float seconds)
	{
		_tickUp = false;
		_timeWaiting = seconds;
		IsRunning = true;
	}

	public void StartCountUp(float startTime)
	{
		_tickUp = true;
		_timeWaiting = startTime;
		IsRunning = true;
	}

	public void StopChronometer()
	{
		IsRunning = false;
	}
}
