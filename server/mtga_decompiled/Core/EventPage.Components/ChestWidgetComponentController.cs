using System;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Providers;

namespace EventPage.Components;

public class ChestWidgetComponentController : IComponentController
{
	private ChestWidgetComponent _component;

	private CardDatabase _cardDatabase;

	private string _cardSkin;

	private CardSkinDatabase _cardSkinDB;

	private CosmeticsProvider _cosmetics;

	public ChestWidgetComponentController(ChestWidgetComponent component, ChestWidgetData data, CardSkinDatabase cardSkinDatabase, CosmeticsProvider cosmetics, Action<DeckBuilderContext> onClick)
	{
		ChestWidgetComponentController chestWidgetComponentController = this;
		_component = component;
		_cardSkin = data.CardSkin;
		_cardSkinDB = cardSkinDatabase;
		_cosmetics = cosmetics;
		DeckbuilderOverrideFactory deckbuilderOverrideFactory = new DeckbuilderOverrideFactory(_cardSkinDB, null);
		_component.Init();
		ChestWidgetComponent component2 = _component;
		component2.OnClick = (Action)Delegate.Combine(component2.OnClick, new Action(componentOnClickedCallback));
		void componentOnClickedCallback()
		{
			DeckBuilderOverride deckBuilderOverride = deckbuilderOverrideFactory.DeckBuilderOverrideForSkin(chestWidgetComponentController._cardSkin);
			onClick?.Invoke(new DeckBuilderContext(null, null, sideboarding: false, firstEdit: false, DeckBuilderMode.ReadOnlyCollection, ambiguousFormat: false, Guid.Empty, deckBuilderOverride.CardPool, deckBuilderOverride.CardSkins));
		}
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
		bool active = false;
		if (!string.IsNullOrWhiteSpace(_cardSkin) && !_cardSkinDB.ArtIdsForSkin(_cardSkin).TrueForAll((uint x) => _cosmetics.OwnsSkin(x, _cardSkin)))
		{
			active = true;
		}
		_component.gameObject.SetActive(active);
	}

	public void Update(IPlayerEvent playerEvent)
	{
	}
}
