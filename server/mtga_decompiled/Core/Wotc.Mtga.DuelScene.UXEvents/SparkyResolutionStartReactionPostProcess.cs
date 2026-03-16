using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using UnityEngine;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtga.Loc;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class SparkyResolutionStartReactionPostProcess : IUXEventGrouper
{
	private readonly IEntityDialogControllerProvider _provider;

	private readonly IClientLocProvider _clientLocProvider;

	private readonly ICardDataProvider _cardDataProvider;

	private readonly IReadOnlyList<MinimumNumberToVOPercentageBuckets> _cmcToVOPercentages;

	private readonly IReadOnlyList<ChatterPair> _cmcCastedChatterOptions;

	private int _prvChatterIdx = -1;

	private List<int> _availableIdx = new List<int>();

	public SparkyResolutionStartReactionPostProcess(IEntityDialogControllerProvider provider, IClientLocProvider clientLocProvider, ICardDataProvider cardDataProvider, IReadOnlyList<MinimumNumberToVOPercentageBuckets> cmcToVOPercentages, IReadOnlyList<ChatterPair> cmcCastedChatterOptions)
	{
		_provider = provider ?? NullEntityDialogControllerProvider.Default;
		_clientLocProvider = clientLocProvider ?? NullLocProvider.Default;
		_cardDataProvider = cardDataProvider ?? NullCardDataProvider.Default;
		_cmcToVOPercentages = cmcToVOPercentages ?? Array.Empty<MinimumNumberToVOPercentageBuckets>();
		_cmcCastedChatterOptions = cmcCastedChatterOptions ?? Array.Empty<ChatterPair>();
	}

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		for (int i = 0; i < events.Count; i++)
		{
			if (events[i] is ResolutionEventStartedUXEvent resolutionEventStartedUXEvent && resolutionEventStartedUXEvent?.Instigator?.Owner?.IsLocalPlayer == true)
			{
				UXEvent uXEvent = GenerateResponse(resolutionEventStartedUXEvent.Instigator);
				if (uXEvent == null)
				{
					break;
				}
				events.Insert(i + 1, uXEvent);
				i++;
			}
		}
	}

	private SparkyChatterUXEvent GenerateResponse(MtgCardInstance instigator)
	{
		if (!_provider.TryGetDialogControllerByPlayerType(GREPlayerNum.Opponent, out var dialogController))
		{
			return null;
		}
		uint id = ((instigator.GrpId == 0) ? instigator.GrpId : instigator.BaseGrpId);
		CardPrintingData cardPrintingById = _cardDataProvider.GetCardPrintingById(id);
		if (cardPrintingById == null)
		{
			return null;
		}
		foreach (MinimumNumberToVOPercentageBuckets cmcToVOPercentage in _cmcToVOPercentages)
		{
			if (cardPrintingById.ConvertedManaCost >= cmcToVOPercentage.minimumNumber && UnityEngine.Random.value < cmcToVOPercentage.percentatageVOPlays)
			{
				return new SparkyChatterUXEvent(_clientLocProvider, dialogController, GetRandomChatterPair());
			}
		}
		return null;
	}

	private ChatterPair GetRandomChatterPair()
	{
		if (_cmcCastedChatterOptions.Count == 0)
		{
			return new ChatterPair();
		}
		int index = (_prvChatterIdx = GetIdx(_prvChatterIdx, _cmcCastedChatterOptions.Count));
		return _cmcCastedChatterOptions[index];
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
