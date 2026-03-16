using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_HasAbilities : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, bb.CardData.Printing.Abilities.Select((AbilityPrintingData x) => (int)x.Id), MinCount, MaxCount);
		}
		return false;
	}
}
