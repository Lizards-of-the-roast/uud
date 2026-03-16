using System;
using System.Collections.Generic;
using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasActiveThresholdAbilityWords : EvaluatorBase_List<string>
{
	public override bool Execute(IBlackboard bb)
	{
		IEnumerable<AbilityWordData> enumerable2;
		if (bb.CardData == null)
		{
			IEnumerable<AbilityWordData> enumerable = Array.Empty<AbilityWordData>();
			enumerable2 = enumerable;
		}
		else
		{
			enumerable2 = bb.CardData.ActiveAbilityWords.Where((AbilityWordData x) => x.Threshold.HasValue);
		}
		IEnumerable<AbilityWordData> source = enumerable2;
		return EvaluatorBase_List<string>.GetResult(ExpectedValues, Operation, ExpectedResult, source.Select((AbilityWordData x) => x.AbilityWord), MinCount, MaxCount);
	}
}
