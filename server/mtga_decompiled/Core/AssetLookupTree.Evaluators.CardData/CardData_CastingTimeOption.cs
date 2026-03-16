using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.Rules;
using Wotc.Mtgo.Gre.External.Messaging;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_CastingTimeOption : EvaluatorBase_List<CastingTimeOptionType>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<CastingTimeOptionType>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.CastingTimeOptions.Select((CastingTimeOption x) => x.Type), MinCount, MaxCount);
		}
		return false;
	}
}
