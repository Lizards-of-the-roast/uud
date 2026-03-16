using AssetLookupTree;
using AssetLookupTree.Payloads.Card.MultistepEffects_FX;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.UXEvents;

public class MultistepEffectCompletedUXEvent : MultistepEffectUXEventBase
{
	public MultistepEffectCompletedUXEvent(MtgPlayer affected, MtgCardInstance affector, AbilitySubCategory abilityCategory, GameManager gameManager)
		: base(affected, affector, abilityCategory, gameManager)
	{
		AssetLookupSystem aLTWithParams = GetALTWithParams(_affectorData);
		MultistepEffect_End_VFX payload = aLTWithParams.TreeLoader.LoadTree<MultistepEffect_End_VFX>().GetPayload(aLTWithParams.Blackboard);
		if (payload != null)
		{
			_vfxs.AddRange(payload.VfxDatas);
		}
		MultistepEffect_End_SFX payload2 = aLTWithParams.TreeLoader.LoadTree<MultistepEffect_End_SFX>().GetPayload(aLTWithParams.Blackboard);
		if (payload2 != null)
		{
			_sfxData = payload2.SfxData;
		}
	}
}
