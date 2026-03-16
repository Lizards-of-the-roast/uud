using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_QualificationReferencedAbilityTypes : EvaluatorBase_List<AbilityType>
{
	public override bool Execute(IBlackboard bb)
	{
		AbilityPrintingData ability;
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<AbilityType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.AffectedByQualifications.Select((QualificationData x) => (bb.CardDatabase.AbilityDataProvider.TryGetAbilityPrintingById(x.AbilityId, out ability), printingData: ability)).SelectMany(delegate((bool, AbilityPrintingData printingData) x)
			{
				IEnumerable<AbilityType> enumerable = x.printingData?.ReferencedAbilityTypes;
				return enumerable ?? Enumerable.Empty<AbilityType>();
			}), MinCount, MaxCount);
		}
		return false;
	}
}
