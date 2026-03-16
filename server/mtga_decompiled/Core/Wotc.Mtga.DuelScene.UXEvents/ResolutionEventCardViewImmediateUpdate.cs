using System.Collections.Generic;
using System.Linq;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ResolutionEventCardViewImmediateUpdate : IUXEventGrouper
{
	private readonly IGameStateProvider _gameStateProvider;

	private readonly ICardDatabaseAdapter _cardDatabase;

	private readonly ICardViewProvider _cardViewProvider;

	public ResolutionEventCardViewImmediateUpdate(IGameStateProvider gameStateProvider, ICardDatabaseAdapter cardDatabase, ICardViewProvider cardViewProvider)
	{
		_gameStateProvider = gameStateProvider;
		_cardDatabase = cardDatabase;
		_cardViewProvider = cardViewProvider;
	}

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		if (events.Exists((UXEvent x) => x is LifeTotalUpdateUXEvent))
		{
			AddLurkingHorrorUpdates(events);
		}
	}

	private void AddLurkingHorrorUpdates(List<UXEvent> events)
	{
		Dictionary<uint, MtgCardInstance> dictionary = _gameStateProvider?.LatestGameState?.Value?.VisibleCards;
		if (dictionary == null)
		{
			return;
		}
		foreach (MtgCardInstance value in dictionary.Values)
		{
			if (_cardDatabase.CardDataProvider.TryGetCardPrintingById(value.GrpId, out var card) && card.Tags.Contains(MetaDataTag.LurkingHorror))
			{
				events.Add(new CardViewImmediateUpdateUXEvent(value.InstanceId, _cardViewProvider));
			}
		}
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
