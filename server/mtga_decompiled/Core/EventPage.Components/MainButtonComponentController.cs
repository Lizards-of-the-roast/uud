using System;
using System.Collections.Generic;
using AssetLookupTree;
using Core.Meta.Tokens;
using Core.Shared.Code.ClientModels;
using Core.Shared.Code.Utilities;
using Wizards.Arena.Enums.Store;
using Wizards.MDN;
using Wizards.Mtga.Format;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Events;
using Wotc.Mtga.Providers;
using Wotc.Mtga.Wrapper.Draft;

namespace EventPage.Components;

public class MainButtonComponentController : IComponentController
{
	private MainButtonComponent _component;

	private ClientPlayerInventory _inventory;

	private AssetLookupSystem _assetLookupSystem;

	private ICustomTokenProvider _customTokenProvider;

	private EventContext _eventContext;

	public MainButtonComponentController(MainButtonComponent component, ClientPlayerInventory inventory, AssetLookupSystem assetLookupSystem, ICustomTokenProvider customTokenProvider, Action<EventEntryFeeInfo> onPayJoinClicked, Action onPlayButtonClicked)
	{
		_component = component;
		_inventory = inventory;
		_assetLookupSystem = assetLookupSystem;
		_customTokenProvider = customTokenProvider;
		_component.PayJoinButton_OnClick = onPayJoinClicked;
		_component.PlayButton_OnClick = onPlayButtonClicked;
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
		_eventContext = eventContext;
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
		switch (state)
		{
		case EventPageStates.DisplayQuest:
		case EventPageStates.ClaimQuestRewards:
			_component.SetButtonForViewState("PlayState", PlayButtonState.Disabled);
			break;
		case EventPageStates.DisplayEvent:
		case EventPageStates.ClaimEventRewards:
			Update(playerEvent);
			break;
		}
	}

	private static void UpdateButtonForEvent(string viewState, EventEntryFeeInfo fee, List<Client_CustomTokenDefinitionWithQty> tokenDefinitions, AssetLookupSystem assetLookupSystem, bool interactable, MainButtonComponent component)
	{
		if (EventHelper.HasTokenForEvent(fee, tokenDefinitions))
		{
			component.SetStateAndShowButton(viewState, fee, interactable, showTooltip: false);
			string text = EventHelper.TokenIdForEvent(fee);
			Client_CustomTokenDefinitionWithQty client_CustomTokenDefinitionWithQty = EventHelper.TokenDefinitionForId(tokenDefinitions, text);
			SimpleLogUtils.LogErrorIfNull(client_CustomTokenDefinitionWithQty, "Could not find token definition for id: " + text);
			string lookupString = client_CustomTokenDefinitionWithQty?.PrefabName;
			component.SetIconForButton(assetLookupSystem, lookupString);
			if (client_CustomTokenDefinitionWithQty != null)
			{
				component.UpdateTextWithQuantity(TokenViewUtilities.GetTokenLocalizationKey(client_CustomTokenDefinitionWithQty.TokenId, client_CustomTokenDefinitionWithQty.HeaderLocKey, fee.Quantity), fee.Quantity);
			}
		}
	}

	public void Update(PlayerEventModule currentModule, bool eventIsActive, bool startingOrRejoiningDraft, bool isLimited, bool hasPrize, List<EventEntryFeeInfo> entryFees)
	{
		_component.ResetButtons();
		switch (currentModule)
		{
		case PlayerEventModule.Join:
		case PlayerEventModule.Pay:
		case PlayerEventModule.PayEntry:
		{
			foreach (EventEntryFeeInfo entryFee in entryFees)
			{
				switch (entryFee.CurrencyType)
				{
				case EventEntryCurrencyType.DraftToken:
					UpdateButtonForEvent("EventState", entryFee, _customTokenProvider.GetCustomTokensOfTypeWithQty(ClientTokenType.Event), _assetLookupSystem, eventIsActive, _component);
					break;
				case EventEntryCurrencyType.EventToken:
					UpdateButtonForEvent("EventState", entryFee, _customTokenProvider.GetCustomTokensOfTypeWithQty(ClientTokenType.Event), _assetLookupSystem, eventIsActive, _component);
					break;
				case EventEntryCurrencyType.Free:
				case EventEntryCurrencyType.None:
					if (entryFee.UsesRemaining)
					{
						_component.SetStateAndShowButton("StartState", entryFee, eventIsActive, showTooltip: false);
					}
					break;
				case EventEntryCurrencyType.Gem:
					_component.SetStateAndShowButton("GemsState", entryFee, entryFee.UsesRemaining && eventIsActive, !entryFee.UsesRemaining);
					_component.UpdateTextWithQuantity(entryFee.Quantity);
					break;
				case EventEntryCurrencyType.Gold:
				{
					bool flag = entryFee.Quantity <= _inventory.gold;
					_component.SetStateAndShowButton("GoldState", entryFee, flag && entryFee.UsesRemaining && eventIsActive, !entryFee.UsesRemaining);
					_component.UpdateTextWithQuantity(entryFee.Quantity);
					break;
				}
				case EventEntryCurrencyType.SealedToken:
					UpdateButtonForEvent("EventState", entryFee, _customTokenProvider.GetCustomTokensOfTypeWithQty(ClientTokenType.Event), _assetLookupSystem, eventIsActive, _component);
					break;
				}
			}
			break;
		}
		case PlayerEventModule.ClaimPrize:
		{
			_component.SetButtonForViewState("PlayState", PlayButtonState.Enabled);
			string locKey3 = (hasPrize ? "MainNav/Rewards/EventRewards/ClaimPrizeButton" : "MainNav/NavBar/NavBar_Home_Text");
			_component.UpdateText(locKey3);
			break;
		}
		case PlayerEventModule.Draft:
		{
			_component.SetButtonForViewState("PlayState", PlayButtonState.Enabled);
			string locKey = (startingOrRejoiningDraft ? "Draft/Start_Draft" : "Draft/Rejoin_Draft");
			_component.UpdateText(locKey);
			break;
		}
		case PlayerEventModule.HumanDraft:
		{
			_component.SetButtonForViewState("PlayState", PlayButtonState.Enabled);
			string locKey4 = (startingOrRejoiningDraft ? "Draft/Start_Draft" : "Draft/Rejoin_Draft");
			_component.UpdateText(locKey4);
			break;
		}
		case PlayerEventModule.Choice:
		case PlayerEventModule.DeckSelect:
		{
			_component.SetButtonForViewState("PlayState", PlayButtonState.Enabled);
			string locKey2 = (isLimited ? "MainNav/EventPage/Button_BuildDeck" : "MainNav/EventPage/Button_SelectDeck");
			_component.UpdateText(locKey2);
			break;
		}
		case PlayerEventModule.TransitionToMatches:
		case PlayerEventModule.WinLossGate:
		case PlayerEventModule.WinNoGate:
			if (EventHelper.PreconWithInvalidDeck(_eventContext?.PlayerEvent))
			{
				_component.SetButtonForViewState("PlayState", PlayButtonState.Enabled);
				_component.UpdateText("MainNav/EventPage/Button_SelectDeck");
			}
			else
			{
				_component.SetButtonForViewState("PlayState", PlayButtonState.Enabled);
				_component.UpdateText("MainNav/EventPage/Button_PlayMatch");
			}
			break;
		case PlayerEventModule.Jumpstart:
			_component.SetButtonForViewState("PlayState", PlayButtonState.Enabled);
			_component.UpdateText("MainNav/EventPage/Button_ChoosePackets");
			break;
		case PlayerEventModule.Complete:
		case PlayerEventModule.NPEUpdate:
			break;
		}
	}

	public void Update(IPlayerEvent playerEvent)
	{
		bool eventIsActive = playerEvent.GetTimerState() != EventTimerState.Preview;
		bool isLimited = FormatUtilities.IsLimited(playerEvent.EventInfo.FormatType);
		bool startingOrRejoiningDraft = IsStartingOrRejoiningDraft(playerEvent);
		bool hasPrize = playerEvent.HasPrize(playerEvent.CurrentWins);
		Update(playerEvent.CourseData.CurrentModule, eventIsActive, startingOrRejoiningDraft, isLimited, hasPrize, playerEvent.EventInfo.EntryFees);
	}

	private bool IsStartingOrRejoiningDraft(IPlayerEvent playerEvent)
	{
		switch (playerEvent.CourseData.CurrentModule)
		{
		case PlayerEventModule.HumanDraft:
		{
			IDraftPod draftPod = (playerEvent as LimitedPlayerEvent).DraftPod;
			if (draftPod == null)
			{
				return false;
			}
			return draftPod.DraftState == DraftState.Podmaking;
		}
		case PlayerEventModule.Draft:
		{
			LimitedPlayerEvent limitedPlayerEvent = playerEvent as LimitedPlayerEvent;
			if (string.IsNullOrEmpty(limitedPlayerEvent.DraftPod?.InternalEventName))
			{
				return string.IsNullOrEmpty(limitedPlayerEvent.DraftPod?.DraftId);
			}
			return false;
		}
		default:
			return false;
		}
	}
}
