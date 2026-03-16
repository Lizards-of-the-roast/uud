using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;
using GreClient.Rules;
using Wotc.Mtga.Cards.Database;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_CastingTimeOptionBaseAbilityIds : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData == null || bb.CardDatabase?.AbilityDataProvider == null)
		{
			return !ExpectedResult;
		}
		IEnumerable<int> inValues = bb.CardData.CastingTimeOptions.Select(delegate(CastingTimeOption x)
		{
			uint valueOrDefault = x.AbilityId.GetValueOrDefault();
			AbilityPrintingData ability;
			return (int)(bb.CardDatabase.AbilityDataProvider.TryGetAbilityPrintingById(valueOrDefault, out ability) ? ability.BaseId : valueOrDefault);
		});
		return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, inValues, MinCount, MaxCount);
	}
}
