using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GrePromptDisplay : MonoBehaviour
{
	[SerializeField]
	private TMP_Text _promptLabel;

	private Coroutine _promptAnimation;

	private float _duration = 3f;

	private const float TEXT_FADE_IN_DURATION = 0.2f;

	private const float TEXT_FADE_OUT_DURATION = 0.4f;

	public void ShowPrompt(string text, float duration)
	{
		if (_promptAnimation != null)
		{
			StopCoroutine(_promptAnimation);
			_promptLabel.alpha = 0f;
		}
		_promptLabel.SetText(text);
		_duration = duration;
		_promptAnimation = StartCoroutine(CoDisplayPrompt());
	}

	private IEnumerator CoDisplayPrompt()
	{
		float timer = 0f;
		float timeInSeconds = 5f;
		while (timer < 1f)
		{
			timer += Time.deltaTime * timeInSeconds;
			_promptLabel.alpha = timer;
			yield return null;
		}
		yield return new WaitForSeconds(_duration - 0.2f - 0.4f);
		timer = 0f;
		timeInSeconds = 2.5f;
		while (timer < 1f)
		{
			timer = Math.Min(timer + Time.deltaTime * timeInSeconds, 1f);
			_promptLabel.alpha = 1f - timer;
			yield return null;
		}
	}
}
