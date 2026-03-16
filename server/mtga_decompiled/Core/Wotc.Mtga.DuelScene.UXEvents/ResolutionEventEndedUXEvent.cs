using System.Collections.Generic;
using AssetLookupTree.Payloads.Resolution;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ResolutionEventEndedUXEvent : ResolutionEffectUXEventBase
{
	public ResolutionEventEndedUXEvent()
	{
	}

	public ResolutionEventEndedUXEvent(IResolutionEffectController resolutionEffectController, MtgCardInstance instigator, CardPrintingData cardPrinting, AbilityPrintingData abilityPrinting, GameManager gameManager, Data resolutionData, IReadOnlyCollection<VFX_Base> vfx, IReadOnlyCollection<SFX_Base> sfx, Projectile projectile, float defaultDuration, float duration)
		: base(resolutionEffectController, instigator, cardPrinting, abilityPrinting, gameManager, resolutionData, vfx, sfx, projectile, defaultDuration, duration)
	{
	}

	protected override void Cleanup()
	{
		base.Cleanup();
		_resolutionEffectController.ResolutionComplete();
	}
}
