using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtga.Extensions;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_Mechanics_IsWaterbending : EvaluatorBase_Boolean
{
	public override bool Execute(IBlackboard bb)
	{
		return EvaluatorBase_Boolean.GetResult(ExpectedResult, IsWaterbending(bb));
		static bool IsCastThroughWaterbend(IBlackboard blackboard)
		{
			if (blackboard.CardData == null || blackboard.CardDatabase?.AbilityDataProvider == null)
			{
				return false;
			}
			return blackboard.CardData.CastingTimeOptions.Exists(blackboard.CardDatabase.AbilityDataProvider, (CastingTimeOption cto, IAbilityDataProvider abilityProvider) => cto.Type == CastingTimeOptionType.CastThroughAbility && cto.AbilityId.HasValue && cto.AbilityId == 386);
		}
		static bool IsWaterbending(IBlackboard blackboard)
		{
			ICardDataAdapter cardData = blackboard.CardData;
			if (cardData == null || cardData.Instance == null)
			{
				return false;
			}
			if (TryGetWaterbendingAbility(blackboard, out var ability))
			{
				if (ability.Tags.Contains(MetaDataTag.Mechanic_IgnoreWaterbendingVFX))
				{
					return false;
				}
				if (ability.Tags.Contains(MetaDataTag.Mechanic_WaterbendX))
				{
					return cardData.CastingTimeOptions.Exists((CastingTimeOption x) => x.Type == CastingTimeOptionType.ChooseX);
				}
				if (ability.Category == AbilityCategory.AdditionalCost)
				{
					if (ability.SubCategory != AbilitySubCategory.CastingTimeOption)
					{
						return true;
					}
					return cardData.CastingTimeOptions.Exists((CastingTimeOption x) => x.AbilityId == ability.Id);
				}
				return true;
			}
			return IsCastThroughWaterbend(blackboard);
		}
		static bool TryGetWaterbendingAbility(IBlackboard blackboard, out AbilityPrintingData ability)
		{
			if (blackboard.Ability != null && blackboard.Ability.ReferencedAbilityTypes.Contains(AbilityType.Waterbend))
			{
				ability = blackboard.Ability;
				return true;
			}
			foreach (AbilityPrintingData ability2 in blackboard.CardData.Abilities)
			{
				if (ability2.ReferencedAbilityTypes.Contains(AbilityType.Waterbend) && ability2.Category == AbilityCategory.AdditionalCost)
				{
					ability = ability2;
					return true;
				}
			}
			ability = AbilityPrintingData.InvalidAbility(0u);
			return false;
		}
	}
}
