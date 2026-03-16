using System.Linq;
using AssetLookupTree.Blackboard;
using GreClient.CardData;

namespace AssetLookupTree.Evaluators.CardData;

public class CardData_HasBaseAbilities : EvaluatorBase_List<int>
{
	public override bool Execute(IBlackboard bb)
	{
		if (bb.CardData != null)
		{
			return EvaluatorBase_List<int>.GetResult(ExpectedValues, Operation, ExpectedResult, from x in bb.CardData.Abilities
				where x.BaseId != 0
				select (int)x.BaseId, MinCount, MaxCount);
		}
		return false;
	}
}
