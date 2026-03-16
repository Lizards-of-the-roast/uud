using System;
using System.Collections;
using Core.Code.Input;
using MTGA.KeyboardManager;
using TMPro;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Core.MatchScene.PreGame;

public class WaitingForMatchView : MonoBehaviour, IKeyDownSubscriber, IKeySubscriber, IBackActionHandler
{
	public enum State
	{
		InQueue,
		JoiningMatch,
		WaitingForOpponent,
		GameFound,
		Minimal,
		Error
	}

	[SerializeField]
	private CustomButton _cancelButton;

	[SerializeField]
	private TMP_Text TimeWaitingText;

	[SerializeField]
	private Localize QueueDescriptionText;

	private TimeSpan _timeSpanPrevious = TimeSpan.MaxValue;

	private float _timeWaiting;

	private PreGameScene _preGameScene;

	private KeyboardManager _keyboardManager;

	private IActionSystem _actionSystem;

	private IEnumerator softlockFailsafeRoutine;

	private float softlockFailsafeTimeout = 70f;

	public PriorityLevelEnum Priority => PriorityLevelEnum.DuelScene_PopUps;

	public void Init(PreGameScene preGameScene, KeyboardManager keyboardManager, IActionSystem actionSystem)
	{
		_keyboardManager = keyboardManager;
		_actionSystem = actionSystem;
		_preGameScene = preGameScene;
		_preGameScene.MatchServiceConnected += OnMatchServiceConnected;
		_preGameScene.MatchConfigReceived += OnMatchConfigReceived;
		_keyboardManager?.Subscribe(this);
		_actionSystem?.PushFocus(this, IActionSystem.Priority.PopUp);
	}

	private void OnMatchServiceConnected()
	{
		SetState(State.WaitingForOpponent);
	}

	private void OnMatchConfigReceived()
	{
		SetState(State.JoiningMatch);
	}

	public void SetState(State newState)
	{
		switch (newState)
		{
		case State.InQueue:
			QueueDescriptionText.SetText("Match/Match_Wait_Time");
			_cancelButton.OnClick.AddListener(delegate
			{
				AudioManager.PlayAudio(WwiseEvents.match_making_cancel, AudioManager.Default);
				_preGameScene.CancelGame();
			});
			break;
		case State.Minimal:
			enableCancelButton(enabled: false);
			break;
		case State.JoiningMatch:
			enableCancelButton(enabled: false);
			QueueDescriptionText.SetText("Match/PreGame/Matchmaking_ConfigFound");
			softlockFailsafeRoutine = DontLetPlayersGetStuckOnVSScreen();
			StartCoroutine(softlockFailsafeRoutine);
			break;
		case State.WaitingForOpponent:
			QueueDescriptionText.SetText("Match/PreGame/Matchmaking_Waiting_For_Opponent");
			break;
		case State.GameFound:
			_cancelButton.Interactable = false;
			ClearSoftlockFailsafe();
			break;
		case State.Error:
			enableCancelButton(enabled: true);
			_cancelButton.OnClick.AddListener(OnSoftLockFailsafeClick);
			softlockFailsafeRoutine = null;
			break;
		}
	}

	private void enableCancelButton(bool enabled)
	{
		_cancelButton.gameObject.UpdateActive(enabled);
		if (!enabled)
		{
			_cancelButton.OnClick.RemoveAllListeners();
		}
	}

	public void Update()
	{
		_timeWaiting += Time.unscaledDeltaTime;
		TimeSpan timeSpanPrevious = TimeSpan.FromSeconds(_timeWaiting);
		if (timeSpanPrevious.Minutes != _timeSpanPrevious.Minutes || timeSpanPrevious.Seconds != _timeSpanPrevious.Seconds)
		{
			string text = $"{timeSpanPrevious.Minutes:D1}:{timeSpanPrevious.Seconds:D2}";
			TimeWaitingText.text = text;
			_timeSpanPrevious = timeSpanPrevious;
		}
	}

	private IEnumerator DontLetPlayersGetStuckOnVSScreen()
	{
		yield return new WaitForSeconds(softlockFailsafeTimeout);
		SetState(State.Error);
	}

	private void OnSoftLockFailsafeClick()
	{
		MatchSceneManager.Instance.ExitMatchScene();
	}

	private void ClearSoftlockFailsafe()
	{
		if (softlockFailsafeRoutine != null)
		{
			StopCoroutine(softlockFailsafeRoutine);
		}
	}

	private void OnDestroy()
	{
		ClearSoftlockFailsafe();
		if (_preGameScene != null)
		{
			_preGameScene.MatchServiceConnected -= OnMatchServiceConnected;
			_preGameScene.MatchConfigReceived -= OnMatchConfigReceived;
		}
		_keyboardManager?.Unsubscribe(this);
		_actionSystem?.PopFocus(this);
	}

	public bool HandleKeyDown(KeyCode curr, Modifiers mods)
	{
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			_cancelButton.OnClick.Invoke();
			return true;
		}
		return false;
	}

	public void OnBack(ActionContext context)
	{
		_cancelButton.OnClick.Invoke();
	}
}
