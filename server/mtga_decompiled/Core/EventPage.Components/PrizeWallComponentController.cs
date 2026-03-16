using AssetLookupTree;
using Core.Code.PrizeWall;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class PrizeWallComponentController : IComponentController
{
	private readonly PrizeWallComponent _component;

	public PrizeWallComponentController(PrizeWallComponent component, EventContext eventContext, PrizeWallDataProvider prizeWallDataProvider, PrizeWallData prizeWallData, AssetLookupSystem assetLookupSystem)
	{
		Client_PrizeWall prizeWallById = prizeWallDataProvider.GetPrizeWallById(prizeWallData.PrizeWallId);
		PrizeWallContext prizeWallContext = new PrizeWallContext(NavContentType.EventLanding, eventContext);
		component.PrizeWallNameLocKey.SetText(prizeWallById.NameLocKey);
		component.CurrencyPrizeWall.SetCurrency(prizeWallDataProvider, prizeWallById, assetLookupSystem);
		_component = component;
		_component.OnClicked = delegate
		{
			SceneLoader.GetSceneLoader().GoToPrizeWall(prizeWallData.PrizeWallId, prizeWallContext);
		};
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void Update(IPlayerEvent playerEvent)
	{
	}
}
