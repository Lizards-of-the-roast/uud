using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TimeoutNotificationDisplay : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _timeoutUsedText;

	private Coroutine _promptAnimation;

	private const float TEXT_FADE_IN_DURATION = 0.2f;

	private const float TEXT_FADE_OUT_DURATION = 0.4f;

	public void DisplayTimeoutNotification()
	{
		if (_promptAnimation != null)
		{
			StopCoroutine(_promptAnimation);
			_timeoutUsedText.alpha = 0f;
		}
		_promptAnimation = StartCoroutine(VISUALTHING());
	}

	private IEnumerator VISUALTHING()
	{
		float timer = 0f;
		float timeInSeconds = 5f;
		while (timer < 1f)
		{
			timer += Time.deltaTime * timeInSeconds;
			_timeoutUsedText.alpha = timer;
			yield return null;
		}
		yield return new WaitForSeconds(0.4f);
		timer = 0f;
		timeInSeconds = 2.5f;
		while (timer < 1f)
		{
			float val = timer + Time.deltaTime * timeInSeconds;
			timer = Math.Min(val, 1f);
			_timeoutUsedText.alpha = 1f - timer;
			yield return null;
		}
	}
}
