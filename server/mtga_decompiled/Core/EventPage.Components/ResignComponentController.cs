using System;
using Assets.Core.Shared.Code;
using Wizards.MDN;
using Wizards.Mtga.FrontDoorModels;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class ResignComponentController : IComponentController
{
	private ResignComponent _component;

	public ResignComponentController(ResignComponent component, Action onClick)
	{
		_component = component;
		ResignComponent component2 = _component;
		component2.OnClick = (Action)Delegate.Combine(component2.OnClick, onClick);
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
		if (state != EventPageStates.DisplayEvent)
		{
			_component.gameObject.UpdateActive(active: false);
		}
		else
		{
			Update(playerEvent);
		}
	}

	public void Update(IPlayerEvent playerEvent)
	{
		PlayerEventModule playerEventModule = playerEvent.CourseData?.CurrentModule ?? PlayerEventModule.None;
		bool flag = false;
		switch (playerEventModule)
		{
		case PlayerEventModule.None:
		case PlayerEventModule.Join:
		case PlayerEventModule.Pay:
		case PlayerEventModule.PayEntry:
		case PlayerEventModule.ClaimPrize:
		case PlayerEventModule.HumanDraft:
		case PlayerEventModule.Jumpstart:
			flag = false;
			break;
		case PlayerEventModule.Draft:
			flag = ServerGameTime.GameTime >= playerEvent.EventInfo.LockedTime;
			break;
		default:
			flag = true;
			break;
		}
		_component.gameObject.UpdateActive(flag);
	}
}
