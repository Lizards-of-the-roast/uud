using System;
using System.Collections.Generic;
using Core.Code.Promises;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wizards.Mtga.Decks;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Network.ServiceWrappers;

namespace EventPage.Components;

public class InspectPreconDecksComponentController : IComponentController
{
	private InspectPreconDecksComponent _component;

	public InspectPreconDecksComponentController(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, InspectPreconDecksComponent component, InspectPreconDecksWidgetData data, IPreconDeckServiceWrapper preconDeckManager, Action<List<Guid>> onClick)
	{
		InspectPreconDecksComponentController inspectPreconDecksComponentController = this;
		_component = component;
		List<Guid> deckIds = data.DeckIds;
		if (deckIds != null && deckIds.Count > 0)
		{
			InspectPreconDecksComponent component2 = _component;
			component2.OnClick = (Action)Delegate.Combine(component2.OnClick, (Action)delegate
			{
				onClick?.Invoke(data.DeckIds);
			});
		}
		else
		{
			_component.gameObject.SetActive(value: false);
		}
		preconDeckManager.EnsurePreconDecks().ThenOnMainThread(delegate(Dictionary<Guid, Client_Deck> decks)
		{
			inspectPreconDecksComponentController.ProcessPreconDecks(cardDatabase, cardViewBuilder, data.DeckIds, decks);
		});
	}

	private void ProcessPreconDecks(CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, IEnumerable<Guid> deckIds, IReadOnlyDictionary<Guid, Client_Deck> decks)
	{
		List<Client_Deck> list = new List<Client_Deck>();
		foreach (Guid deckId in deckIds)
		{
			if (decks.TryGetValue(deckId, out var value))
			{
				list.Add(value);
			}
		}
		_component.SetDecks(cardDatabase, cardViewBuilder, list);
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void Update(IPlayerEvent playerEvent)
	{
		bool active = ShowDecksForModule(playerEvent.CourseData.CurrentModule);
		_component.gameObject.SetActive(active);
	}

	private static bool ShowDecksForModule(PlayerEventModule currentModule)
	{
		if (currentModule != PlayerEventModule.TransitionToMatches)
		{
			return currentModule != PlayerEventModule.Choice;
		}
		return false;
	}
}
