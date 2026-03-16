using System;
using System.Collections.Generic;
using UnityEngine;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class SparkyDamagedPostProcess : IUXEventGrouper
{
	private readonly IClientLocProvider _clientLocProvider;

	private readonly IEntityDialogControllerProvider _provider;

	private readonly IReadOnlyList<MinimumNumberToVOPercentageBuckets> _damageToVOPercentage;

	private readonly IReadOnlyList<ChatterPair> _damageTakenChatterOptions;

	private int _prvChatterIdx = -1;

	private List<int> _availableIdx = new List<int>();

	public SparkyDamagedPostProcess(IClientLocProvider clientLocProvider, IEntityDialogControllerProvider provider, IReadOnlyList<MinimumNumberToVOPercentageBuckets> damageToVOPercentage, IReadOnlyList<ChatterPair> damageTakenChatterOptions)
	{
		_clientLocProvider = clientLocProvider;
		_provider = provider ?? NullEntityDialogControllerProvider.Default;
		_damageToVOPercentage = damageToVOPercentage ?? Array.Empty<MinimumNumberToVOPercentageBuckets>();
		_damageTakenChatterOptions = damageTakenChatterOptions ?? Array.Empty<ChatterPair>();
	}

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		if (!SparkyIsAlive(events))
		{
			return;
		}
		(int, int) eventParams = GetEventParams(events);
		if (eventParams.Item1 != -1 && eventParams.Item2 > 0)
		{
			UXEvent uXEvent = GenerateResponse(eventParams.Item2);
			if (uXEvent != null)
			{
				events.Insert(eventParams.Item1, uXEvent);
			}
		}
	}

	private bool SparkyIsAlive(IEnumerable<UXEvent> events)
	{
		foreach (UXEvent @event in events)
		{
			if (@event is GameStatePlaybackCommencedUXEvent gameStatePlaybackCommencedUXEvent && gameStatePlaybackCommencedUXEvent.GameState.Opponent.LifeTotal > 0)
			{
				return true;
			}
		}
		return false;
	}

	private (int insertIdx, int damageDealt) GetEventParams(IReadOnlyList<UXEvent> events)
	{
		int num = 0;
		int item = -1;
		for (int i = 0; i < events.Count; i++)
		{
			if (events[i] is CombatFrame combatFrame)
			{
				num += combatFrame.OpponentDamageDealt;
				item = i + 1;
			}
		}
		return (insertIdx: item, damageDealt: num);
	}

	private SparkyChatterUXEvent GenerateResponse(int totalDamageDealt)
	{
		if (!_provider.TryGetDialogControllerByPlayerType(GREPlayerNum.Opponent, out var dialogController))
		{
			return null;
		}
		foreach (MinimumNumberToVOPercentageBuckets item in _damageToVOPercentage)
		{
			if (totalDamageDealt >= item.minimumNumber && UnityEngine.Random.value < item.percentatageVOPlays)
			{
				return new SparkyChatterUXEvent(_clientLocProvider, dialogController, GetRandomChatterPair(), isBlocking: true);
			}
		}
		return null;
	}

	private ChatterPair GetRandomChatterPair()
	{
		if (_damageTakenChatterOptions.Count == 0)
		{
			return new ChatterPair();
		}
		int index = (_prvChatterIdx = GetIdx(_prvChatterIdx, _damageTakenChatterOptions.Count));
		return _damageTakenChatterOptions[index];
	}

	private int GetIdx(int prvIdx, int listCount)
	{
		_availableIdx.Clear();
		for (int i = 0; i < listCount; i++)
		{
			if (prvIdx != i)
			{
				_availableIdx.Add(i);
			}
		}
		int result = _availableIdx.SelectRandom();
		_availableIdx.Clear();
		return result;
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
