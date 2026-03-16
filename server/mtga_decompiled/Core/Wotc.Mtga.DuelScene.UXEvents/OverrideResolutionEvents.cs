using System.Collections.Generic;
using AssetLookupTree;
using Wotc.Mtga.DuelScene.VFX;
using Wotc.Mtga.Extensions;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class OverrideResolutionEvents<UxEvent> : IUXEventGrouper where UxEvent : UXEvent
{
	private readonly AssetLookupSystem _assetLookupSystem;

	private readonly IVfxProvider _vfxProvider;

	public OverrideResolutionEvents(AssetLookupSystem assetLookupSystem, IVfxProvider vfxProvider)
	{
		_assetLookupSystem = assetLookupSystem;
		_vfxProvider = vfxProvider;
	}

	public void GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		int num = events.FindIndex((UXEvent x) => x is ResolutionEventStartedUXEvent resolutionEventStartedUXEvent && resolutionEventStartedUXEvent.IgnoreDamageEvents);
		if (num != -1)
		{
			int num2 = events.FindIndex(num + 1, (UXEvent x) => x is ResolutionEventEndedUXEvent);
			num2 = ((num2 == -1) ? events.Count : (num2 - 1));
			int num3 = events.FindIndex(num + 1, (UXEvent x) => x is UxEvent);
			if (num3 > -1)
			{
				UxEvent overrideEvent = events[num3] as UxEvent;
				ResolutionEventStartedUXEvent resolutionEvent = events[num] as ResolutionEventStartedUXEvent;
				events.Insert(num3, CreateOverrideEvent(resolutionEvent, overrideEvent));
			}
			events.RemoveRange(num + 1, num2, (UXEvent x) => x is UxEvent);
		}
	}

	private ResolutionOverrideUXEvent CreateOverrideEvent(ResolutionEffectUXEventBase resolutionEvent, UxEvent overrideEvent)
	{
		return new ResolutionOverrideUXEvent(resolutionEvent.InstigatorModel, resolutionEvent.AbilityPrinting, overrideEvent, _assetLookupSystem, _vfxProvider);
	}

	void IUXEventGrouper.GroupEvents(in int startIdx, ref List<UXEvent> events)
	{
		GroupEvents(in startIdx, ref events);
	}
}
