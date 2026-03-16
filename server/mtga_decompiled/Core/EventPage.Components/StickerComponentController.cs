using System.Collections.Generic;
using AssetLookupTree;
using EventPage.Components.NetworkModels;
using Wizards.MDN;
using Wotc.Mtga.Events;

namespace EventPage.Components;

public class StickerComponentController : IComponentController
{
	private StickerComponent _component;

	public StickerComponentController(StickerComponent component, StickerDisplayData data, IEmoteDataProvider emoteDataProvider, AssetLookupSystem assetLookupSystem)
	{
		_component = component;
		List<string> stickers = data.Stickers;
		if (stickers != null && stickers.Count > 0)
		{
			_component.SetStickers(data.Stickers, emoteDataProvider, assetLookupSystem);
		}
		else
		{
			_component.gameObject.SetActive(value: false);
		}
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
