using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Evaluators.Ability;

public class Ability_AllActiveTags : EvaluatorBase_List<MetaDataTag>
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_List<MetaDataTag>.GetResult(ExpectedValues, Operation, ExpectedResult, GetTags(bb.CardData, bb.Ability), MinCount, MaxCount);
	}

	private static IEnumerable<MetaDataTag> GetTags(ICardDataAdapter cardData, AbilityPrintingData ability)
	{
		if (ability == null)
		{
			yield break;
		}
		if (ability.IsModalAbility())
		{
			foreach (MetaDataTag activeModalTag in GetActiveModalTags(cardData, ability))
			{
				yield return activeModalTag;
			}
			yield break;
		}
		if (ability.IsModalAbilityChild() && ability.TryGetModalAbilityParent(cardData.Abilities, out var parent))
		{
			foreach (MetaDataTag activeModalTag2 in GetActiveModalTags(cardData, parent))
			{
				yield return activeModalTag2;
			}
			yield break;
		}
		foreach (MetaDataTag tag in ability.Tags)
		{
			yield return tag;
		}
	}

	private static IEnumerable<MetaDataTag> GetActiveModalTags(ICardDataAdapter cardData, AbilityPrintingData parent)
	{
		foreach (MetaDataTag tag in parent.Tags)
		{
			yield return tag;
		}
		foreach (AbilityPrintingData modalAbilityChild in parent.ModalAbilityChildren)
		{
			if (!cardData.Abilities.ContainsId(modalAbilityChild.Id))
			{
				continue;
			}
			foreach (MetaDataTag tag2 in modalAbilityChild.Tags)
			{
				yield return tag2;
			}
		}
	}
}
