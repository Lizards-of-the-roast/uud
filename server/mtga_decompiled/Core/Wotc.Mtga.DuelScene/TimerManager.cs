using System.Collections.Generic;
using AssetLookupTree;
using AssetLookupTree.Payloads.UI;
using GreClient.Rules;
using UnityEngine;
using Wizards.Mtga.Platforms;
using Wotc.Mtga.Duel;
using Wotc.Mtga.DuelScene.Browsers;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene;

public class TimerManager
{
	private TimeoutNotificationDisplay _timeoutNotificationDisplay;

	private PlayerTimeoutDisplay _localPlayerTimeoutDisplay;

	private PlayerTimeoutDisplay _opponentTimeoutDisplay;

	private MatchTimer _localPlayerMatchTimer;

	private MatchTimer _opponentMatchTimer;

	private LowTimeWarning _localPlayerTimeWarning;

	private LowTimeWarning _opponentTimeWarning;

	private LowTimeFrameGlow _lowTimeFrameGlow;

	private Coroutine _localPlayerTimeWarningMoveCoroutine;

	private Coroutine _opponentTimeWarningMoveCoroutine;

	private GREPlayerNum _decidingPlayer;

	public PlayerTimeoutDisplay LocalPlayerTimeoutDisplay => _localPlayerTimeoutDisplay;

	public PlayerTimeoutDisplay OpponentPlayerTimeoutDisplay => _opponentTimeoutDisplay;

	public MatchTimer LocalPlayerMatchTimer => _localPlayerMatchTimer;

	public MatchTimer OpponentMatchTimer => _opponentMatchTimer;

	public TimerManager(CanvasManager canvasManager, BrowserManager browserManager, UIManager uiManager, AssetLookupSystem assetLookupSystem)
	{
		assetLookupSystem.Blackboard.Clear();
		TimerPrefabs payload = assetLookupSystem.TreeLoader.LoadTree<TimerPrefabs>(returnNewTree: false).GetPayload(assetLookupSystem.Blackboard);
		if (payload == null)
		{
			Debug.LogError("Timer Prefab Payload is null");
			return;
		}
		_timeoutNotificationDisplay = AssetLoader.Instantiate(payload.TimeoutVisualsRef, canvasManager.GetCanvasRoot(CanvasLayer.Overlay));
		PlayerNames playerNames = uiManager.PlayerNames;
		if ((object)playerNames != null)
		{
			TimerViewAndTimeoutViewReferences component = playerNames.gameObject.GetComponent<TimerViewAndTimeoutViewReferences>();
			if ((object)component != null)
			{
				_localPlayerTimeoutDisplay = component.LocalTimeoutView;
				_opponentTimeoutDisplay = component.OpponentTimeoutView;
				_localPlayerMatchTimer = component.LocalTimerView;
				_opponentMatchTimer = component.OpponentTimerView;
				goto IL_00fe;
			}
		}
		Transform canvasRoot = canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_Default);
		_localPlayerTimeoutDisplay = AssetLoader.Instantiate(payload.LocalPlayerTimeoutDisplayRef, canvasRoot);
		_opponentTimeoutDisplay = AssetLoader.Instantiate(payload.OpponentTimeoutDisplayRef, canvasRoot);
		_localPlayerMatchTimer = AssetLoader.Instantiate(payload.LocalPlayerMatchTimerRef, canvasRoot);
		_opponentMatchTimer = AssetLoader.Instantiate(payload.OpponentMatchTimerRef, canvasRoot);
		goto IL_00fe;
		IL_00fe:
		Transform transform = new GameObject("LowTimeWarnings").transform;
		_localPlayerTimeWarning = AssetLoader.Instantiate(payload.LocalPlayerLowTimeWarningRef, transform);
		_localPlayerTimeWarning.BrowserManager = browserManager;
		_localPlayerTimeWarning.OnVisibilityChanged.AddListener(HandleLowTimeWarningVisibilityChanged);
		_opponentTimeWarning = AssetLoader.Instantiate(payload.OpponentLowTimeWarningRef, transform);
		_opponentTimeWarning.BrowserManager = browserManager;
		_opponentTimeWarning.OnVisibilityChanged.AddListener(HandleLowTimeWarningVisibilityChanged);
		_lowTimeFrameGlow = AssetLoader.Instantiate(parent: (!PlatformUtils.IsHandheld()) ? canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_UnderStack) : canvasManager.GetCanvasRoot(CanvasLayer.ScreenSpace_UnderStack, "FullScreenEffect", allowScreenSafe: false), assetReference: payload.LowTimeFrameGlowRef);
	}

	public void DisableView()
	{
		_localPlayerTimeoutDisplay.gameObject.UpdateActive(active: false);
		_opponentTimeoutDisplay.gameObject.UpdateActive(active: false);
	}

	public void UpdateTimers(MtgPlayer localPlayer, MtgPlayer opponent, GameInfo gameInfo)
	{
		if (gameInfo.MaxTimeoutCount == 0)
		{
			DisableView();
		}
		MtgTimer activeTimer = GetActiveTimer(localPlayer.Timers);
		MtgTimer inactiveTimer = GetInactiveTimer(localPlayer.Timers);
		MtgTimer matchClockTimer = GetMatchClockTimer(localPlayer.Timers);
		uint timeoutCount = localPlayer.TimeoutCount;
		_localPlayerTimeWarning.SetModel(activeTimer, inactiveTimer, matchClockTimer, timeoutCount);
		_localPlayerTimeoutDisplay.SetTimeoutCount(timeoutCount);
		_localPlayerMatchTimer.SetGameInfo(gameInfo);
		_localPlayerMatchTimer.SetMatchTimer(matchClockTimer);
		_lowTimeFrameGlow.UpdateTimer(activeTimer);
		MtgTimer activeTimer2 = GetActiveTimer(opponent.Timers);
		MtgTimer inactiveTimer2 = GetInactiveTimer(opponent.Timers);
		MtgTimer matchClockTimer2 = GetMatchClockTimer(opponent.Timers);
		uint timeoutCount2 = opponent.TimeoutCount;
		_opponentTimeWarning.SetModel(activeTimer2, inactiveTimer2, matchClockTimer2, timeoutCount2);
		_opponentTimeoutDisplay.SetTimeoutCount(timeoutCount2);
		_opponentMatchTimer.SetGameInfo(gameInfo);
		_opponentMatchTimer.SetMatchTimer(matchClockTimer2);
	}

	public void UpdateTimer(MtgPlayer player)
	{
		List<MtgTimer> timers = player.Timers;
		MtgTimer activeTimer = GetActiveTimer(timers);
		MtgTimer inactiveTimer = GetInactiveTimer(timers);
		MtgTimer matchClockTimer = GetMatchClockTimer(timers);
		bool isLocalPlayer = player.IsLocalPlayer;
		uint timeoutCount = player.TimeoutCount;
		(isLocalPlayer ? _localPlayerTimeWarning : _opponentTimeWarning).SetModel(activeTimer, inactiveTimer, matchClockTimer, timeoutCount);
		(isLocalPlayer ? _localPlayerMatchTimer : _opponentMatchTimer).SetMatchTimer(matchClockTimer);
		if (isLocalPlayer)
		{
			_lowTimeFrameGlow.UpdateTimer(activeTimer);
		}
	}

	private MtgTimer GetActiveTimer(IReadOnlyCollection<MtgTimer> timers)
	{
		foreach (MtgTimer timer in timers)
		{
			if (timer.TimerType != TimerType.Delay && (timer.TimerType != TimerType.Inactivity || timers.Count <= 1) && timer.RemainingTime != 0f && (timer.IsPaused || timer.Running))
			{
				return timer;
			}
		}
		return null;
	}

	private MtgTimer GetInactiveTimer(IEnumerable<MtgTimer> timers)
	{
		foreach (MtgTimer timer in timers)
		{
			if (timer.TimerType == TimerType.Inactivity)
			{
				return timer;
			}
		}
		return null;
	}

	private MtgTimer GetMatchClockTimer(IEnumerable<MtgTimer> timers)
	{
		foreach (MtgTimer timer in timers)
		{
			if (timer.TimerType == TimerType.MatchClock)
			{
				return timer;
			}
		}
		return null;
	}

	public void DisplayTimeoutNotification(MtgTimer updatedTimer, bool triggeredByLocalPlayer, uint timeoutCount)
	{
		(triggeredByLocalPlayer ? _localPlayerTimeWarning : _opponentTimeWarning).OnTimeout(updatedTimer);
		(triggeredByLocalPlayer ? _localPlayerTimeoutDisplay : _opponentTimeoutDisplay).SetTimeoutCount(timeoutCount);
		if (triggeredByLocalPlayer)
		{
			_lowTimeFrameGlow.UpdateTimer(updatedTimer);
		}
		_timeoutNotificationDisplay.DisplayTimeoutNotification();
	}

	public int GetVisibleLowTimeWarningsCount()
	{
		int num = 0;
		if (_localPlayerTimeWarning.ActiveandVis)
		{
			num++;
		}
		if (_opponentTimeWarning.ActiveandVis)
		{
			num++;
		}
		return num;
	}

	private void HandleLowTimeWarningVisibilityChanged(bool isVisible)
	{
		LowTimeWarning_Handheld localWarning = _localPlayerTimeWarning as LowTimeWarning_Handheld;
		LowTimeWarning_Handheld opponentWarning = _opponentTimeWarning as LowTimeWarning_Handheld;
		if (!(localWarning == null) && !(opponentWarning == null))
		{
			int visibleLowTimeWarningsCount = GetVisibleLowTimeWarningsCount();
			if (isVisible && visibleLowTimeWarningsCount > 1)
			{
				StopLowTimeWarningMoveCoroutines();
				_localPlayerTimeWarningMoveCoroutine = localWarning.StartCoroutine(localWarning.AdjustPosition(isExpanding: true));
				_opponentTimeWarningMoveCoroutine = opponentWarning.StartCoroutine(opponentWarning.AdjustPosition(isExpanding: true));
			}
			else if (!isVisible && visibleLowTimeWarningsCount <= 1)
			{
				StopLowTimeWarningMoveCoroutines();
				_localPlayerTimeWarningMoveCoroutine = localWarning.StartCoroutine(localWarning.AdjustPosition(isExpanding: false));
				_opponentTimeWarningMoveCoroutine = opponentWarning.StartCoroutine(opponentWarning.AdjustPosition(isExpanding: false));
			}
			UpdateLowTimeWarningHourGlasses();
		}
		void StopLowTimeWarningMoveCoroutines()
		{
			if (_localPlayerTimeWarningMoveCoroutine != null)
			{
				localWarning.StopCoroutine(_localPlayerTimeWarningMoveCoroutine);
			}
			if (_opponentTimeWarningMoveCoroutine != null)
			{
				opponentWarning.StopCoroutine(_opponentTimeWarningMoveCoroutine);
			}
		}
	}

	public void UpdateDecidingPlayer(GREPlayerNum playerNum)
	{
		_decidingPlayer = playerNum;
		UpdateLowTimeWarningHourGlasses();
	}

	private void UpdateLowTimeWarningHourGlasses()
	{
		LowTimeWarning_Handheld lowTimeWarning_Handheld = _localPlayerTimeWarning as LowTimeWarning_Handheld;
		LowTimeWarning_Handheld lowTimeWarning_Handheld2 = _opponentTimeWarning as LowTimeWarning_Handheld;
		if (!(lowTimeWarning_Handheld == null) && !(lowTimeWarning_Handheld2 == null) && _decidingPlayer != GREPlayerNum.Invalid)
		{
			if (GetVisibleLowTimeWarningsCount() <= 1)
			{
				lowTimeWarning_Handheld.ToggleHourGlass(isEnabled: true);
				lowTimeWarning_Handheld2.ToggleHourGlass(isEnabled: true);
				return;
			}
			bool isEnabled = _decidingPlayer == GREPlayerNum.LocalPlayer;
			bool isEnabled2 = _decidingPlayer == GREPlayerNum.Opponent;
			lowTimeWarning_Handheld.ToggleHourGlass(isEnabled);
			lowTimeWarning_Handheld2.ToggleHourGlass(isEnabled2);
		}
	}

	public bool IsLocalPlayerTimeWarningVisible()
	{
		return _localPlayerTimeWarning.ActiveandVis;
	}
}
