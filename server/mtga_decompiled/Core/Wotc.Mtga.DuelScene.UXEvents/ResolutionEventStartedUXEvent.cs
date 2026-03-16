using System.Collections.Generic;
using AssetLookupTree.Payloads.Resolution;
using GreClient.CardData;
using GreClient.Rules;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class ResolutionEventStartedUXEvent : ResolutionEffectUXEventBase
{
	private readonly ResolutionEffectModel _resolutionEffectModel;

	public ResolutionEventStartedUXEvent()
	{
	}

	public ResolutionEventStartedUXEvent(ResolutionEffectModel resolutionEffectModel, IResolutionEffectController resolutionEffectController, MtgCardInstance instigator, CardPrintingData cardPrinting, AbilityPrintingData abilityPrinting, GameManager gameManager, Data resolutionData, IReadOnlyCollection<VFX_Base> vfx, IReadOnlyCollection<SFX_Base> sfx, Projectile projectile, float defaultDuration, float duration)
		: base(resolutionEffectController, instigator, cardPrinting, abilityPrinting, gameManager, resolutionData, vfx, sfx, projectile, defaultDuration, duration)
	{
		_resolutionEffectModel = resolutionEffectModel;
	}

	public ResolutionEventStartedUXEvent(MtgCardInstance instigator)
		: base(instigator)
	{
	}

	public override void Execute()
	{
		_resolutionEffectController.ResolutionStart(_resolutionEffectModel);
		base.Execute();
	}
}
