using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData.Printing;

public class CardData_Printing_HasBaseAbilities : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData?.Printing != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, from x in bb.CardData.Printing.Abilities
				where x.BaseId != 0
				select (int)x.BaseId, MinCount, MaxCount);
		}
		return false;
	}
}
