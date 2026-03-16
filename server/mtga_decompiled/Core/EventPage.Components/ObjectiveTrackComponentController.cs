using EventPage.Components.NetworkModels;
using UnityEngine;
using Wizards.MDN;
using Wizards.Mtga.FrontDoorModels;
using Wizards.Mtga.Inventory;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class ObjectiveTrackComponentController : IComponentController
{
	private ObjectiveTrackComponent _component;

	private ClientPlayerInventory _inventory;

	public ObjectiveTrackComponentController(ObjectiveTrackComponent component, ObjectiveTrackData trackData, RectTransform safeArea, CardDatabase cardDatabase, CardViewBuilder cardViewBuilder, CardMaterialBuilder cardMaterialBuilder)
	{
		_component = component;
		_component.safeArea = safeArea;
		_component.SetRewardData(trackData.ChestDescriptions, cardDatabase, cardViewBuilder, cardMaterialBuilder);
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
		int currentWins = eventContext.PlayerEvent.CurrentWins;
		PostMatchContext postMatchContext = eventContext.PostMatchContext;
		int previousWins = currentWins - ((postMatchContext != null && postMatchContext.WonGame) ? 1 : 0);
		_component.UpdateWins(currentWins, previousWins);
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
		switch (state)
		{
		case EventPageStates.DisplayQuest:
		case EventPageStates.ClaimQuestRewards:
			_component.gameObject.UpdateActive(active: false);
			break;
		case EventPageStates.DisplayEvent:
		{
			bool active = playerEvent.CourseData.CurrentModule == PlayerEventModule.Join || playerEvent.CourseData.CurrentModule == PlayerEventModule.PayEntry;
			_component.SetActive(active);
			break;
		}
		}
	}

	public void Update(IPlayerEvent playerEvent)
	{
	}
}
