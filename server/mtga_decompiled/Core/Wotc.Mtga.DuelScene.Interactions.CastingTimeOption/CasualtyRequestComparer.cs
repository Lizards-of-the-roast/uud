using System.Collections.Generic;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace Wotc.Mtga.DuelScene.Interactions.CastingTimeOption;

public class CasualtyRequestComparer : IComparer<BaseUserRequest>
{
	private readonly IAbilityDataProvider _abilityDataProvider;

	public CasualtyRequestComparer(IAbilityDataProvider abilityDataProvider)
	{
		_abilityDataProvider = abilityDataProvider ?? new NullAbilityDataProvider();
	}

	public int Compare(BaseUserRequest x, BaseUserRequest y)
	{
		if (x is CastingTimeOption_CostKeywordRequest castingTimeOption_CostKeywordRequest && y is CastingTimeOption_CostKeywordRequest castingTimeOption_CostKeywordRequest2)
		{
			AbilityPrintingData abilityPrintingById = _abilityDataProvider.GetAbilityPrintingById(castingTimeOption_CostKeywordRequest.GrpId);
			AbilityPrintingData abilityPrintingById2 = _abilityDataProvider.GetAbilityPrintingById(castingTimeOption_CostKeywordRequest2.GrpId);
			if (abilityPrintingById == null || abilityPrintingById2 == null)
			{
				return (abilityPrintingById == null).CompareTo(abilityPrintingById2 == null);
			}
			if (abilityPrintingById.BaseId != abilityPrintingById2.BaseId)
			{
				return abilityPrintingById.Id.CompareTo(abilityPrintingById2.Id);
			}
			return abilityPrintingById.BaseIdNumeral.GetValueOrDefault().CompareTo(abilityPrintingById2.BaseIdNumeral.GetValueOrDefault());
		}
		return (y is CastingTimeOption_DoneRequest).CompareTo(x is CastingTimeOption_DoneRequest);
	}
}
