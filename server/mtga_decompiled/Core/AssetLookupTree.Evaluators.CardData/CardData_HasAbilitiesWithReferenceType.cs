using System.Collections.Generic;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasAbilitiesWithReferenceType : EvaluatorBase_List<AbilityType>
{
	private readonly HashSet<AbilityType> _referenceTypes = new HashSet<AbilityType>();

	public override bool Execute(IBlackboard bb)
	{
		_referenceTypes.Clear();
		if (bb.CardData == null || bb.CardData?.Abilities == null)
		{
			return false;
		}
		foreach (AbilityPrintingData ability in bb.CardData.Abilities)
		{
			_referenceTypes.UnionWith(ability.ReferencedAbilityTypes);
			foreach (AbilityPrintingData hiddenAbility in ability.HiddenAbilities)
			{
				_referenceTypes.UnionWith(hiddenAbility.ReferencedAbilityTypes);
			}
		}
		bool result = EvaluatorBase_List<AbilityType>.GetResult(ExpectedValues, Operation, ExpectedResult, _referenceTypes, MinCount, MaxCount);
		_referenceTypes.Clear();
		return result;
	}
}
