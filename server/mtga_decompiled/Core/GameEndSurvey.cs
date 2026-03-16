using System;
using UnityEngine;

public class GameEndSurvey : MonoBehaviour
{
	[SerializeField]
	private CustomButton _buttonBad;

	[SerializeField]
	private CustomButton _buttonGood;

	[SerializeField]
	private CustomButton _buttonSkip;

	[SerializeField]
	private CustomButton _clickShield;

	private const string FEEDBACK_NONE = "NoFeedback";

	private const string FEEDBACK_GOOD = "Good";

	private const string FEEDBACK_BAD = "Bad";

	private const string FEEDBACK_SKIPPED = "IntentionallySkipped";

	public Action<string> FeedbackSubmitted;

	private void Awake()
	{
		_buttonBad.OnClick.AddListener(onClick);
		_buttonBad.OnMouseover.AddListener(onHover);
		_buttonGood.OnClick.AddListener(onClick2);
		_buttonGood.OnMouseover.AddListener(onHover2);
		_buttonSkip.OnClick.AddListener(onClick3);
		_buttonSkip.OnMouseover.AddListener(onHover3);
		_clickShield.OnClick.AddListener(onClick4);
		void onClick()
		{
			FeedbackSubmitted?.Invoke("Bad");
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
			ClearButtonCallbacks();
		}
		void onClick2()
		{
			FeedbackSubmitted?.Invoke("Good");
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
			ClearButtonCallbacks();
		}
		void onClick3()
		{
			FeedbackSubmitted?.Invoke("IntentionallySkipped");
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_accept, base.gameObject);
			ClearButtonCallbacks();
		}
		void onClick4()
		{
			FeedbackSubmitted?.Invoke("NoFeedback");
			ClearButtonCallbacks();
		}
		void onHover()
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
		}
		void onHover2()
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
		}
		void onHover3()
		{
			AudioManager.PlayAudio(WwiseEvents.sfx_ui_main_rollover, base.gameObject);
		}
	}

	public void ForceClose()
	{
		FeedbackSubmitted?.Invoke("NoFeedback");
		ClearButtonCallbacks();
		FeedbackSubmitted = null;
	}

	private void OnDestroy()
	{
		ClearButtonCallbacks();
		FeedbackSubmitted = null;
	}

	private void ClearButtonCallbacks()
	{
		_buttonBad.OnClick.RemoveAllListeners();
		_buttonBad.OnMouseover.RemoveAllListeners();
		_buttonGood.OnClick.RemoveAllListeners();
		_buttonGood.OnMouseover.RemoveAllListeners();
		_buttonSkip.OnClick.RemoveAllListeners();
		_buttonSkip.OnMouseover.RemoveAllListeners();
		_clickShield.OnClick.RemoveAllListeners();
	}
}
