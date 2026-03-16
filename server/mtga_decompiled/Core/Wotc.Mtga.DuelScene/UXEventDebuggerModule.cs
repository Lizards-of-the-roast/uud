using System;
using UnityEngine;
using Wotc.Mtga.DuelScene.UXEvents;

namespace Wotc.Mtga.DuelScene;

public class UXEventDebuggerModule : DebugModule, IDisposable
{
	private readonly UXEventQueue _eventQueue;

	private bool _pauseOnNewUpdate;

	private bool _pauseOnRunEvent;

	public override string Name => "UXEvent Debugger";

	public override string Description => "Allows for granular control over the playback of gamestate events";

	public UXEventDebuggerModule(UXEventQueue eventQueue)
	{
		_eventQueue = eventQueue ?? new UXEventQueue();
		_eventQueue.EventExecutionCommenced += OnEventCommenced;
	}

	private void OnEventCommenced(UXEvent uxEvent)
	{
		if (_pauseOnRunEvent || (_pauseOnNewUpdate && uxEvent is GameStatePlaybackCommencedUXEvent))
		{
			_eventQueue.Pause();
		}
	}

	public override void Render()
	{
		_pauseOnNewUpdate = GUILayout.Toggle(_pauseOnNewUpdate, "Pause on update");
		_pauseOnRunEvent = GUILayout.Toggle(_pauseOnRunEvent, "Pause on run");
		if (GUILayout.Button("Next"))
		{
			_eventQueue.Resume();
		}
		GUILayout.Label("Running:");
		foreach (UXEvent runningEvent in _eventQueue.RunningEvents)
		{
			GUILayout.Label(runningEvent.ToString());
		}
		GUILayout.Label("Pending:");
		foreach (UXEvent pendingEvent in _eventQueue.PendingEvents)
		{
			GUILayout.Label(pendingEvent.ToString());
		}
	}

	public void Dispose()
	{
		_eventQueue.EventExecutionCommenced -= OnEventCommenced;
	}
}
