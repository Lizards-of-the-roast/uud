using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Indirectors;

public class InteractionSource_Ability : IIndirector
{
	private AbilityPrintingData _cachedAbilityData;

	public IEnumerable<IBlackboard> Execute(IBlackboard bb)
	{
		if (bb.GameState == null || bb.Interaction == null || bb.CardDataProvider == null)
		{
			yield break;
		}
		MtgCardInstance card = null;
		if (bb.Interaction.SourceId != 0)
		{
			bb.GameState.TryGetCard(bb.Interaction.SourceId, out card);
		}
		else
		{
			card = bb.GameState.GetTopCardOnStack();
		}
		if (card == null)
		{
			yield break;
		}
		if (card.ObjectType == GameObjectType.Ability)
		{
			AbilityPrintingData abilityPrintingById = bb.AbilityDataProvider.GetAbilityPrintingById(card.GrpId);
			if (abilityPrintingById != null)
			{
				bb.Ability = abilityPrintingById;
				yield return bb;
			}
			yield break;
		}
		MtgCardInstance mtgCardInstance = card.Children.Find((MtgCardInstance x) => x.ObjectType == GameObjectType.Ability && x.Zone.Type == ZoneType.Stack);
		if (mtgCardInstance != null)
		{
			AbilityPrintingData abilityPrintingById2 = bb.AbilityDataProvider.GetAbilityPrintingById(mtgCardInstance.GrpId);
			if (abilityPrintingById2 != null)
			{
				bb.Ability = abilityPrintingById2;
				yield return bb;
			}
		}
	}

	public void SetCache(IBlackboard bb)
	{
		_cachedAbilityData = bb.Ability;
	}

	public void ClearCache(IBlackboard bb)
	{
		bb.Ability = _cachedAbilityData;
		_cachedAbilityData = null;
	}
}
