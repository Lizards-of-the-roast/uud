using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Indirectors.CardData.Printing;

public class CardData_GameWideCountAbility : IIndirector
{
	private AbilityPrintingData _abilityCache;

	public void SetCache(IBlackboard bb)
	{
		_abilityCache = bb.Ability;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Ability = _abilityCache;
		_abilityCache = null;
	}

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		ICardDataAdapter cardData = bb.CardData;
		if (cardData == null)
		{
			yield break;
		}
		MtgCardInstance instance = cardData.Instance;
		if (instance == null)
		{
			yield break;
		}
		foreach (GamewideCountData gamewideCount in instance.GamewideCounts)
		{
			if (bb.AbilityDataProvider.TryGetAbilityPrintingById(gamewideCount.AbilityId, out var ability))
			{
				bb.Ability = ability;
				yield return bb;
			}
		}
	}
}
