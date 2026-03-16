using System;
using HasbroGo.Wands;
using HasbroGoUnity.Wands;
using UnityEngine;

namespace HasbroGo;

public class WizWandsManager : MonoBehaviour
{
	[SerializeField]
	private bool IsPerformanceHeartBeatEnabled = true;

	[SerializeField]
	private float PerformanceHeartBeatTimer = 30f;

	[SerializeField]
	private float InactiveTimer = 120f;

	private DateTime _activeSessionStartTime = DateTime.UtcNow;

	private DateTime _pausedSessionTimer = DateTime.UtcNow;

	private bool _hasActiveSessionRecorded;

	public static WizWandsManager Instance { get; private set; }

	private HasbroGoSDK _sdk => HasbroGoSDKManager.Instance.HasbroGoSdk;

	private void Awake()
	{
		if (Instance != null && Instance != this)
		{
			UnityEngine.Object.Destroy(this);
			return;
		}
		Instance = this;
		UnityEngine.Object.DontDestroyOnLoad(this);
		WizWandsEvents.SetHasbroGoSdk(_sdk.WizWands);
		WizWandsEvents.CreatePlayerHeader(string.Empty);
		_sdk.WizWands.GetFlushingEventRegistry().AddPostAppendFlushingEventType(typeof(EventData_PlaySessionEnd));
		_sdk.WizWands.GetFlushingEventRegistry().AddPostAppendFlushingEventType(typeof(EventData_PerformanceHeartbeat));
		WizWandsEvents.RecordClientSessionStart();
		if (IsPerformanceHeartBeatEnabled)
		{
			InvokeRepeating("PerformanceHeartBeat", 0f, PerformanceHeartBeatTimer);
		}
		HasbroGoSDKManager.Instance.OnShutdownEvent += ShutDownWands;
	}

	private void PerformanceHeartBeat()
	{
		WizWandsEvents.RecordPerformanceHeartBeat();
	}

	private void OnApplicationPause(bool pause)
	{
		if (pause)
		{
			if (!_hasActiveSessionRecorded)
			{
				_hasActiveSessionRecorded = true;
				_pausedSessionTimer = DateTime.UtcNow;
				RecordActiveSession((DateTime.UtcNow - _activeSessionStartTime).TotalMilliseconds);
			}
			return;
		}
		if ((DateTime.UtcNow - _pausedSessionTimer).TotalSeconds > (double)InactiveTimer)
		{
			WizWandsEvents.UpdateClientSessionIdWithNewId(Guid.NewGuid().ToString());
			WizWandsEvents.RecordClientSessionStart();
		}
		_hasActiveSessionRecorded = false;
		_activeSessionStartTime = DateTime.UtcNow;
	}

	private void RecordActiveSessionQuit()
	{
		if (!_hasActiveSessionRecorded)
		{
			RecordActiveSession((DateTime.UtcNow - _activeSessionStartTime).TotalMilliseconds);
		}
	}

	private void RecordActiveSession(double seconds)
	{
		WandsHelper.RecordCustomEvent(seconds, "CustomDouble");
	}

	private void ShutDownWands()
	{
		RecordActiveSessionQuit();
		WizWandsEvents.RecordClientSessionEnd();
		HasbroGoSDKManager.Instance.OnShutdownEvent -= ShutDownWands;
	}
}
