using System.Collections.Generic;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class LossDetailsComponentController : IComponentController
{
	private LossDetailsComponent _component;

	public LossDetailsComponentController(LossDetailsComponent component, LossDetailsDisplayData data)
	{
		_component = component;
		if (data.LossDetailsType == LossDetailsType.LossSlots)
		{
			_component.CreateLossSlots(data.Games);
		}
		else
		{
			_component.HideLossSlots();
		}
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void Update(IPlayerEvent playerEvent)
	{
		LossDetailsDisplayData lossDetailsDisplay = playerEvent.EventUXInfo.EventComponentData.LossDetailsDisplay;
		if (lossDetailsDisplay == null)
		{
			return;
		}
		bool flag = false;
		bool flag2 = false;
		switch (playerEvent.CourseData.CurrentModule)
		{
		case PlayerEventModule.Join:
		case PlayerEventModule.Pay:
		case PlayerEventModule.PayEntry:
			flag = true;
			break;
		case PlayerEventModule.TransitionToMatches:
		case PlayerEventModule.WinLossGate:
		case PlayerEventModule.WinNoGate:
		case PlayerEventModule.ClaimPrize:
			flag2 = true;
			break;
		}
		switch (lossDetailsDisplay.LossDetailsType)
		{
		case LossDetailsType.LossSlots:
			if (flag2)
			{
				Dictionary<string, string> parameters3 = new Dictionary<string, string> { 
				{
					"qty",
					playerEvent.CurrentLosses.ToString()
				} };
				string key2 = ((playerEvent.CurrentLosses == 1) ? "MainNav/EventPage/LossText_Singular" : "MainNav/EventPage/LossText_Plural");
				MTGALocalizedString text3 = new MTGALocalizedString
				{
					Key = key2,
					Parameters = parameters3
				};
				_component.UpdateLossSlots(playerEvent.CurrentLosses);
				_component.ShowNumberOfLossesText(text3);
			}
			else
			{
				Dictionary<string, string> parameters4 = new Dictionary<string, string> { 
				{
					"count",
					lossDetailsDisplay.Games.ToString()
				} };
				string key3 = ((lossDetailsDisplay.Games == 1) ? "Events/Event_PlayUntil_Loss" : "Events/Event_PlayUntil_Losses");
				MTGALocalizedString text4 = new MTGALocalizedString
				{
					Key = key3,
					Parameters = parameters4
				};
				_component.UpdateLossSlots(0);
				_component.ShowEventEndCriteriaText(text4);
			}
			break;
		case LossDetailsType.PlayMaxGames:
			if (flag)
			{
				Dictionary<string, string> parameters = new Dictionary<string, string> { 
				{
					"matchCount",
					lossDetailsDisplay.Games.ToString()
				} };
				MTGALocalizedString text = new MTGALocalizedString
				{
					Key = "MainNav/EventPage/MaxMatches",
					Parameters = parameters
				};
				_component.ShowEventEndCriteriaText(text);
			}
			else
			{
				int num = lossDetailsDisplay.Games - playerEvent.GamesPlayed;
				Dictionary<string, string> parameters2 = new Dictionary<string, string> { 
				{
					"number1",
					num.ToString()
				} };
				string key = ((num == 1) ? "MainNav/EventPage/RemainingGamesCountSingular" : "MainNav/EventPage/RemainingGamesCountPlural");
				MTGALocalizedString text2 = new MTGALocalizedString
				{
					Key = key,
					Parameters = parameters2
				};
				_component.ShowEventEndCriteriaText(text2);
			}
			break;
		case LossDetailsType.PlayUntilEventEnds:
			_component.ShowEventEndCriteriaText("Events/Event_PlayUntil_EventEnd");
			break;
		}
	}
}
