using System;
using AssetLookupTree;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wotc.Mtga.Events;
using Wotc.Mtga.Extensions;

namespace EventPage.Components;

public class TitleRankComponentController : IComponentController
{
	private TitleRankComponent _component;

	private AssetLookupSystem _assetLookupSystem;

	public TitleRankComponentController(TitleRankComponent component, AssetLookupSystem assetLookupSystem, Action<bool> backButtonClicked)
	{
		_component = component;
		_assetLookupSystem = assetLookupSystem;
		_component.BackButtonAction = backButtonClicked;
	}

	public void OnEventPageStateChanged(IPlayerEvent playerEvent, EventPageStates state)
	{
	}

	public void OnEventPageOpen(EventContext eventContext)
	{
	}

	public void Update(IPlayerEvent playerEvent)
	{
		bool active = false;
		LocalizedTextData titleRankText = playerEvent.EventUXInfo.EventComponentData.TitleRankText;
		if (titleRankText != null)
		{
			active = true;
			_component.SetText(titleRankText.LocKey);
			if (playerEvent.EventInfo.IsRanked)
			{
				_component.ShowRank(playerEvent.EventInfo.FormatType, _assetLookupSystem);
			}
			else
			{
				_component.HideRank();
			}
		}
		_component.ShowBackButton(playerEvent.EventUXInfo.OpenedFromPlayBlade);
		_component.gameObject.UpdateActive(active);
	}
}
