using System;
using Wizards.Arena.Enums.UILayout;
using Wizards.MDN;
using Wizards.Mtga.Decks;
using Wizards.Mtga.Format;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Unification.Models.Events;
using Wotc.Mtga.Events;
using Wotc.Mtga.Loc;

namespace EventPage.Components;

public class SelectedDeckComponentController : IComponentController
{
	private readonly SelectedDeckComponent _component;

	private readonly Action<Guid, Action<Client_Deck>> _submitDeckCallback;

	private readonly Action<LayoutDeckButtonBehavior> _onDeckBoxClicked;

	public SelectedDeckComponentController(SelectedDeckComponent component, Action<Guid, Action<Client_Deck>> submitDeck, Action<LayoutDeckButtonBehavior> onDeckBoxClicked, Action onCopyToDecksClicked, Action onSelectDeckClicked)
	{
		_component = component;
		_submitDeckCallback = submitDeck;
		_onDeckBoxClicked = onDeckBoxClicked;
		_component.OnCopyToDecksClicked = onCopyToDecksClicked;
		_component.OnSelectDeckClicked = onSelectDeckClicked;
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void Update(IPlayerEvent playerEvent)
	{
		SelectedDeckWidgetData data = playerEvent.EventUXInfo.EventComponentData.SelectedDeckWidget;
		if (data == null)
		{
			return;
		}
		PlayerEventModule currentModule = playerEvent.CourseData.CurrentModule;
		if (currentModule == PlayerEventModule.Choice || currentModule == PlayerEventModule.DeckSelect)
		{
			LayoutDeckButtonBehavior deckButtonBehavior = data.DeckButtonBehavior;
			if (deckButtonBehavior == LayoutDeckButtonBehavior.Fixed || deckButtonBehavior == LayoutDeckButtonBehavior.Selectable)
			{
				_component.ShowSelectDeckButton();
			}
			else
			{
				_component.gameObject.SetActive(value: false);
			}
			return;
		}
		Client_Deck courseDeck = playerEvent.CourseData.CourseDeck;
		bool flag = playerEvent.GetTimerState() != EventTimerState.ClosedAndCompleted;
		bool flag2 = FormatUtilities.IsLimited(playerEvent.EventInfo.FormatType);
		bool flag3 = currentModule == PlayerEventModule.ClaimPrize;
		if (courseDeck != null && ((playerEvent.InPlayingMatchesModule && flag) || (flag3 && flag2)))
		{
			switch (data.DeckButtonBehavior)
			{
			case LayoutDeckButtonBehavior.Selectable:
				if (playerEvent.EventInfo.IsPreconEvent)
				{
					if (!playerEvent.EventUXInfo.EventComponentData.InspectPreconDecksWidget.DeckIds.Contains(courseDeck.Id))
					{
						_component.ShowSelectDeckButton();
						break;
					}
					courseDeck.Summary.Name = Utils.GetLocalizedDeckName(courseDeck.Summary.Name);
					_component.UpdateDeckBoxUI(courseDeck, "MainNav/EventPage/Button_SelectDeck", enabled: true, data.ShowCopyDeckButton, onDeckBoxClicked);
					break;
				}
				_component.gameObject.SetActive(value: false);
				if (_submitDeckCallback == null)
				{
					break;
				}
				_submitDeckCallback(courseDeck.Id, delegate(Client_Deck deck)
				{
					if (deck != null)
					{
						_component.UpdateDeckBoxUI(deck, "MainNav/EventPage/Button_SelectDeck", enabled: true, data.ShowCopyDeckButton, onDeckBoxClicked);
					}
				});
				break;
			case LayoutDeckButtonBehavior.Fixed:
				courseDeck.Summary.Name = playerEvent.GetDeckName(Languages.ActiveLocProvider);
				_component.UpdateDeckBoxUI(courseDeck, "MainNav/ConstructedDeckSelect/Tooltip_InspectDeck", enabled: true, data.ShowCopyDeckButton, onDeckBoxClicked);
				break;
			case LayoutDeckButtonBehavior.Editable:
				_component.UpdateDeckBoxUI(courseDeck, "MainNav/EventPage/Tooltip_ClickToEditDeck", enabled: true, data.ShowCopyDeckButton, onDeckBoxClicked);
				break;
			default:
				_component.UpdateDeckBoxUI(courseDeck, "MainNav/General/Empty_String", enabled: false, data.ShowCopyDeckButton, null);
				break;
			}
		}
		else
		{
			_component.gameObject.SetActive(value: false);
		}
		void onDeckBoxClicked()
		{
			_onDeckBoxClicked?.Invoke(data.DeckButtonBehavior);
		}
	}
}
