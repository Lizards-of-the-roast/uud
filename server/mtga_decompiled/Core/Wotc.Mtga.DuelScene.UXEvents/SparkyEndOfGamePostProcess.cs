using System;
using System.Collections.Generic;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class SparkyEndOfGamePostProcess : IUXEventGrouper
{
	private readonly IClientLocProvider _clientLocProvider;

	private readonly IEntityDialogControllerProvider _provider;

	private readonly IReadOnlyList<ChatterPair> _playerLoseChatterOptions;

	private readonly IReadOnlyList<ChatterPair> _sparkyLoseChatterOptions;

	public SparkyEndOfGamePostProcess(IClientLocProvider clientLocProvider, IEntityDialogControllerProvider provider, IReadOnlyList<ChatterPair> playerLoseChatterOptions, IReadOnlyList<ChatterPair> sparkyLoseChatterOptions)
	{
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
		_provider = provider ?? NullEntityDialogControllerProvider.Default;
		_playerLoseChatterOptions = playerLoseChatterOptions ?? Array.Empty<ChatterPair>();
		_sparkyLoseChatterOptions = sparkyLoseChatterOptions ?? Array.Empty<ChatterPair>();
	}

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		(int, bool) tuple = EventParameters(events);
		if (tuple.Item1 != -1)
		{
			events.Insert(tuple.Item1, GenerateResponse(tuple.Item2));
		}
	}

	private (int insertIdx, bool sparkyWon) EventParameters(IReadOnlyList<UXEvent> events)
	{
		for (int i = 0; i < events.Count; i++)
		{
			if (events[i] is GameEndUXEvent { Result: ResultType.WinLoss } gameEndUXEvent)
			{
				return (insertIdx: i + 1, sparkyWon: gameEndUXEvent.Loser == GREPlayerNum.LocalPlayer);
			}
		}
		return (insertIdx: -1, sparkyWon: false);
	}

	private SparkyChatterUXEvent GenerateResponse(bool sparkyWon)
	{
		if (!_provider.TryGetDialogControllerByPlayerType(GREPlayerNum.Opponent, out var dialogController))
		{
			return null;
		}
		IReadOnlyList<ChatterPair> sequence = (sparkyWon ? _playerLoseChatterOptions : _sparkyLoseChatterOptions);
		return new SparkyChatterUXEvent(_clientLocProvider, dialogController, sequence.SelectRandom(), isBlocking: true);
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
