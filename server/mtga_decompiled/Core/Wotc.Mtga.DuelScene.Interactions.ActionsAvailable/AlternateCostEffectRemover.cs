using System;
using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace Wotc.Mtga.DuelScene.Interactions.ActionsAvailable;

public class AlternateCostEffectRemover : IClientSideOptionModelMutator
{
	public readonly ICardDataAdapter AltCostOptionModel;

	public readonly uint AltCostEffectAbilityId;

	private readonly List<(uint abilityId, uint textIdOverride)> _abilityIdsCache = new List<(uint, uint)>(10);

	public AlternateCostEffectRemover(ICardDataAdapter altCostOptionModel, AbilityPrintingData altCostAbility)
	{
		AltCostOptionModel = altCostOptionModel;
		if (altCostAbility == null || altCostAbility.Category != AbilityCategory.AlternativeCost)
		{
			return;
		}
		uint id = altCostAbility.Id;
		if (id <= 141919)
		{
			if (id != 141890)
			{
				_ = 141919;
			}
			else
			{
				AltCostEffectAbilityId = 141891u;
			}
			return;
		}
		switch (id)
		{
		default:
			_ = 142031;
			break;
		case 141991u:
			AltCostEffectAbilityId = 141992u;
			break;
		case 141942u:
			AltCostEffectAbilityId = 141943u;
			break;
		}
	}

	public bool Mutate(ref ICardDataAdapter targetModel)
	{
		if (AltCostOptionModel == targetModel)
		{
			return false;
		}
		if (targetModel == null)
		{
			return false;
		}
		if (AltCostEffectAbilityId == 0)
		{
			return false;
		}
		MtgCardInstance instance = targetModel.Instance;
		bool flag = false;
		for (int num = instance.Abilities.Count - 1; num >= 0; num--)
		{
			if (instance.Abilities[num].Id == AltCostEffectAbilityId)
			{
				flag = true;
				instance.Abilities.RemoveAt(num);
			}
		}
		CardPrintingData printing = targetModel.Printing;
		_abilityIdsCache.Clear();
		_abilityIdsCache.AddRange(printing.AbilityIds);
		bool flag2 = false;
		for (int num2 = _abilityIdsCache.Count - 1; num2 >= 0; num2--)
		{
			if (_abilityIdsCache[num2].abilityId == AltCostEffectAbilityId)
			{
				flag2 = true;
				_abilityIdsCache.RemoveAt(num2);
			}
		}
		if (flag2)
		{
			CardPrintingData other = printing;
			CardPrintingRecord record = printing.Record;
			IReadOnlyList<(uint, uint)> abilityIds = ((_abilityIdsCache.Count > 0) ? ((IReadOnlyList<(uint, uint)>)_abilityIdsCache.ToArray()) : ((IReadOnlyList<(uint, uint)>)Array.Empty<(uint, uint)>()));
			printing = new CardPrintingData(other, new CardPrintingRecord(record, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, abilityIds));
			targetModel = new CardData(instance, printing);
		}
		return flag || flag2;
	}
}
