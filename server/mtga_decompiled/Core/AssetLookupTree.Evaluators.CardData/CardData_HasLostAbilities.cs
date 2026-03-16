using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasLostAbilities : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.RemovedAbilities.Select((AbilityPrintingData x) => (int)x.Id), MinCount, MaxCount);
		}
		return false;
	}
}
