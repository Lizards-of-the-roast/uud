using System;
using System.Collections.Generic;
using GreClient.Rules;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class CullEventsAfterConcedePostProcess : IUXEventGrouper
{
	private readonly UXEventQueue _eventQueue;

	public CullEventsAfterConcedePostProcess(UXEventQueue eventQueue)
	{
		_eventQueue = eventQueue ?? new UXEventQueue();
	}

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		if (CullEndGameEvents(GetGameStateFromEvents(events)))
		{
			events.RemoveAll(RemoveEndGameEvents);
			_eventQueue.ClearPendingAndRunningIfNotInitialGameState();
		}
	}

	private MtgGameState GetGameStateFromEvents(IEnumerable<UXEvent> events)
	{
		foreach (UXEvent item in events ?? Array.Empty<UXEvent>())
		{
			if (item is GameStatePlaybackCommencedUXEvent gameStatePlaybackCommencedUXEvent)
			{
				return gameStatePlaybackCommencedUXEvent.GameState;
			}
		}
		return new MtgGameState();
	}

	public static bool CullEndGameEvents(MtgGameState state)
	{
		if (state.Type == MtgGameState.StateType.None && state.Stage == GameStage.GameOver)
		{
			return state.GameInfo.ResultForCurrentGame().Reason == ResultReason.Concede;
		}
		return false;
	}

	public static bool RemoveEndGameEvents(UXEvent evt)
	{
		if (!(evt is GameEndUXEvent))
		{
			if (!(evt is GameStatePlaybackCommencedUXEvent))
			{
				if (!(evt is GameStatePlaybackCompletedUXEvent))
				{
					if (evt is UpdateCardModelUXEvent { Property: var property })
					{
						switch (property)
						{
						case PropertyType.Anonymity:
							return false;
						case PropertyType.FaceDown:
							return false;
						case PropertyType.Visibility:
							return false;
						case PropertyType.Actions:
							return false;
						}
					}
					else
					{
						if (evt is UXEventGroup uXEventGroup)
						{
							return uXEventGroup.Events.TrueForAll(RemoveEndGameEvents);
						}
						if (evt is ParallelPlaybackUXEvent parallelPlaybackUXEvent)
						{
							return parallelPlaybackUXEvent.Events.TrueForAll(RemoveEndGameEvents);
						}
					}
					return true;
				}
				return false;
			}
			return false;
		}
		return false;
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
