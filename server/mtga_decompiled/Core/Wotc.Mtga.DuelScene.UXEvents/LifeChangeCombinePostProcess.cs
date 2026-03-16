using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class LifeChangeCombinePostProcess : IUXEventGrouper
{
	private const uint ABIL_ID_EACH_CREATURE_DAMAGES_CONTROLLER = 30085u;

	private readonly IGameStateProvider _gameStateProvider;

	public LifeChangeCombinePostProcess(IGameStateProvider gameStateProvider)
	{
		_gameStateProvider = gameStateProvider;
	}

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		if (!CanGroupEvents(GetResolvingCardInstance(events, _gameStateProvider.LatestGameState)))
		{
			return;
		}
		for (int i = startIdx; i < events.Count; i++)
		{
			if (!(events[i] is LifeTotalUpdateUXEvent lifeTotalUpdateUXEvent))
			{
				continue;
			}
			bool flag = lifeTotalUpdateUXEvent.Change > 0;
			int num = lifeTotalUpdateUXEvent.Change;
			int num2 = i + 1;
			while (num2 < events.Count && events[num2] is LifeTotalUpdateUXEvent lifeTotalUpdateUXEvent2 && lifeTotalUpdateUXEvent.AffectedId == lifeTotalUpdateUXEvent2.AffectedId)
			{
				bool flag2 = lifeTotalUpdateUXEvent2.Change > 0;
				if (flag != flag2)
				{
					break;
				}
				num += lifeTotalUpdateUXEvent2.Change;
				events.RemoveAt(num2);
			}
			if (num != lifeTotalUpdateUXEvent.Change)
			{
				lifeTotalUpdateUXEvent.SetChangeValue(num);
			}
		}
	}

	private MtgCardInstance GetResolvingCardInstance(IReadOnlyList<UXEvent> events, MtgGameState gameState)
	{
		foreach (UXEvent @event in events)
		{
			if (@event is ResolutionEventStartedUXEvent resolutionEventStartedUXEvent)
			{
				return resolutionEventStartedUXEvent.Instigator;
			}
		}
		return gameState.ResolvingCardInstance;
	}

	private bool CanGroupEvents(MtgCardInstance resolvingInstance)
	{
		return resolvingInstance?.Abilities.Exists(30085u, (AbilityPrintingData x, uint abilityId) => x.Id == abilityId) ?? false;
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
